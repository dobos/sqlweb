using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Configuration;

namespace Jhu.SqlWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                RefreshQueries();
                RefreshBrowser();
            }
        }

        protected void Execute_Click(object sender, EventArgs e)
        {
            try
            {
                System.IO.StringWriter output = new System.IO.StringWriter();

                using (SqlConnection cn = OpenConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(Query.Text, cn))
                    {
                        cmd.CommandTimeout = 10;

                        using (SqlDataReader dr = cmd.ExecuteReader())
                        {
                            output.WriteLine("<table cellspacing=\"0\" rules=\"all\" border=\"1\" style=\"border-collapse:collapse;\">");
                            output.WriteLine("<tr>");

                            for (int i = 0; i < dr.FieldCount; i++)
                            {
                                output.WriteLine("<td>{0}<br />{1}</td>", dr.GetName(i), dr.GetDataTypeName(i));
                            }

                            output.WriteLine("</tr>");

                            int c = 0;
                            while (dr.Read() && c < 1000)
                            {
                                output.WriteLine("<tr>");

                                for (int i = 0; i < dr.FieldCount; i++)
                                {
                                    output.WriteLine("<td nowrap>{0}</td>", dr.GetValue(i).ToString());
                                }

                                output.WriteLine("</tr>");

                                c++;
                            }

                            output.WriteLine("</table>");
                        }
                    }
                }

                resultsDiv.Visible = true;
                planDiv.Visible = false;
                ResultsGrid.Text = output.ToString();

                StatusReady();
            }
            catch (Exception ex)
            {
                StatusError(ex);
            }
        }

        protected void Plan_Click(object sender, EventArgs e)
        {
            // GRANT SHOWPLAN TO [user]

            try
            {
                using (SqlConnection cn = OpenConnection())
                {
                    using (var cmd = new SqlCommand("SET SHOWPLAN_XML ON", cn))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    using (SqlCommand cmd = new SqlCommand(Query.Text, cn))
                    {
                        cmd.CommandTimeout = 10;
                        var plan = (string)cmd.ExecuteScalar();
                        planXml.Value = plan;
                    }

                    using (var cmd = new SqlCommand("SET SHOWPLAN_XML OFF", cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                resultsDiv.Visible = false;
                planDiv.Visible = true;

                StatusReady();
            }
            catch (Exception ex)
            {
                StatusError(ex);
            }
        }

        protected void Syntax_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection cn = OpenConnection())
                {
                    using (SqlCommand cmd = new SqlCommand(Query.Text, cn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                StatusGood("Syntax OK");
            }
            catch (Exception ex)
            {
                StatusError(ex);
            }
        }

        private void StatusError(Exception ex)
        {
            Status.Text = String.Format("Syntax error: {0}", ex.Message);
            Status.BackColor = Color.Red;
            Status.ForeColor = Color.White;
        }

        private void StatusReady()
        {
            Status.Text = "Ready";
            Status.BackColor = Color.Transparent;
            Status.ForeColor = Color.Black;
        }

        private void StatusGood(string message)
        {
            Status.Text = message;
            Status.BackColor = Color.Green;
            Status.ForeColor = Color.White;
        }

        protected void Refresh_Click(object sender, EventArgs e)
        {
            RefreshBrowser();
        }

        private void RefreshQueries()
        {
            Samples.Items.Clear();
            Samples.Items.Add(new ListItem("(select sample query)", "(select sample query)"));

            var files = Directory.GetFiles(Server.MapPath("queries"), "*.sql");

            foreach (var file in files)
            {
                var fn = Path.GetFileNameWithoutExtension(file);
                var q = File.ReadAllText(file);

                Samples.Items.Add(new ListItem(fn, q));
            }
        }

        private void RefreshBrowser()
        {
            BrowserTree.Nodes.Clear();

            TreeNode tables = new TreeNode("Tables");
            LoadSqlObjects(tables, "U");
            BrowserTree.Nodes.Add(tables);
            tables.CollapseAll();

            TreeNode views = new TreeNode("Views");
            LoadSqlObjects(views, "V");
            BrowserTree.Nodes.Add(views);
            views.CollapseAll();

            TreeNode sps = new TreeNode("Stored procedures");
            LoadSqlObjects(sps, "P");
            BrowserTree.Nodes.Add(sps);
            sps.CollapseAll();
        }

        private void LoadSqlObjects(TreeNode node, string type)
        {
            using (SqlConnection cn = OpenConnection())
            {
                string sql = @"
SELECT o.object_id, s.name, o.name
FROM sys.objects o
INNER JOIN sys.schemas s ON o.schema_id = s.schema_id
WHERE type = '{0}'
ORDER BY s.name, o.name";

                sql = String.Format(sql, type);

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            int o = -1;

                            int id = dr.GetInt32(++o);
                            string sname = dr.GetString(++o);
                            string oname = dr.GetString(++o);

                            TreeNode tn = new TreeNode(String.Format("{0}.{1}", sname, oname), id.ToString());
                            node.ChildNodes.Add(tn);

                            switch (type)
                            {
                                case "U":
                                case "V":
                                    LoadColumns(tn, id, cn);
                                    break;
                                case "P":
                                    LoadParameters(tn, id, cn);
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }
            }
        }

        private void LoadColumns(TreeNode node, int objectid, SqlConnection cn)
        {
            TreeNode tn = new TreeNode("Columns");
            node.ChildNodes.Add(tn);

            node = tn;

            string sql = @"
SELECT c.object_id, c.column_id, c.name, t.name
FROM sys.columns c
INNER JOIN sys.types t ON t.system_type_id = c.system_type_id AND t.user_type_id = c.user_type_id
WHERE c.object_id = {0}";

            sql = String.Format(sql, objectid);

            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int o = -1;

                        int id = dr.GetInt32(++o);
                        int cid = dr.GetInt32(++o);
                        string cname = dr.GetString(++o);
                        string tname = dr.GetString(++o);

                        tn = new TreeNode(String.Format("{0} {1}", cname, tname), String.Format("{0}:{1}", id, cid));
                        node.ChildNodes.Add(tn);
                    }
                }
            }
        }

        private void LoadParameters(TreeNode node, int objectid, SqlConnection cn)
        {
            TreeNode tn = new TreeNode("Parameters");
            node.ChildNodes.Add(tn);

            node = tn;

            string sql = @"
SELECT p.object_id, p.parameter_id, p.name, t.name
FROM sys.parameters p
INNER JOIN sys.types t ON t.system_type_id = p.system_type_id AND t.user_type_id = p.user_type_id
WHERE p.object_id = {0}";

            sql = String.Format(sql, objectid);

            using (SqlCommand cmd = new SqlCommand(sql, cn))
            {
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        int o = -1;

                        int id = dr.GetInt32(++o);
                        int pid = dr.GetInt32(++o);
                        string pname = dr.GetString(++o);
                        string tname = dr.GetString(++o);

                        tn = new TreeNode(String.Format("{0} {1}", pname, tname), String.Format("{0}:{1}", id, pid));
                        node.ChildNodes.Add(tn);
                    }
                }
            }
        }

        private SqlConnection OpenConnection()
        {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["TheConnection"].ConnectionString);
            csb.MultipleActiveResultSets = true;

            SqlConnection cn = new SqlConnection(csb.ConnectionString);
            cn.Open();
            return cn;
        }

        protected void Samples_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(Samples.SelectedValue))
            {
                Query.Text = Samples.SelectedValue;
            }

            Samples.SelectedIndex = 0;
        }
    }
}