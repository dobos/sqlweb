﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="Jhu.SqlWeb.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <title></title>
    <script src="editor/lib/codemirror.js"></script>
    <script src="editor/mode/mysql/mysql.js"></script>
    <script type="text/javascript" language="javascript">
        function LoadWindow() {
            ResizeWindow();
        }

        function ResizeWindow() {
            var fw = window.innerWidth;
            var fh = window.innerHeight;

            var tb = document.getElementById("toolbarDiv").style;
            tb.left = 4 + "px";
            tb.top = 4 + "px";
            tb.width = (fw - 8) + "px";

            var st = document.getElementById("statusDiv").style;
            st.left = 4 + "px";
            st.top = (parseInt(tb.top) + parseInt(tb.height) + 4) + "px";
            st.width = (fw - 8) + "px";

            var br = document.getElementById("browserDiv").style;
            br.left = 4 + "px";
            br.width = (fw * 0.3) + "px";
            br.top = (parseInt(st.top) + parseInt(st.height) + 4) + "px";
            br.height = (fh - parseInt(br.top) - 4) + "px";

            var qu = document.getElementById("queryDiv").style;
            qu.left = (parseInt(br.left) + parseInt(br.width) + 4) + "px";
            qu.width = (fw - parseInt(qu.left) - 4) + "px";
            qu.top = parseInt(br.top) + "px";
            qu.height = ((parseInt(br.height) - 4) / 2) + "px";

            var re = document.getElementById("resultsDiv").style;
            re.left = parseInt(qu.left) + "px";
            re.width = parseInt(qu.width) + "px";
            re.top = (parseInt(qu.top) + parseInt(qu.height) + 4) + "px";
            re.height = (fh - parseInt(re.top) - 4) + "px";

            tb.visibility = "visible";
            st.visibility = "visible";
            br.visibility = "visible";
            qu.visibility = "visible";
            re.visibility = "visible";
        }

        window.onload = LoadWindow;
        window.onresize = ResizeWindow;
    </script>
    <link rel="stylesheet" href="editor/lib/codemirror.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div style="position: absolute; visibility: hidden; top: 4px; left: 4px; width: 925px;
        height: 24px;" id="toolbarDiv">
        <asp:Button ID="Refresh" runat="server" Text="Refresh" OnClick="Refresh_Click" />
        &nbsp;|
        <asp:Button ID="Syntax" runat="server" Text="Syntax" OnClick="Syntax_Click" />
        &nbsp;<asp:Button ID="Execute" runat="server" Text="Execute" OnClick="Execute_Click" />
        &nbsp;<asp:Button ID="Plan" runat="server" Enabled="False" Text="Plan" />
        |
        <asp:DropDownList runat="server" ID="Samples" AutoPostBack="true" OnSelectedIndexChanged="Samples_SelectedIndexChanged">
            <asp:ListItem>(select sample query)</asp:ListItem>
        </asp:DropDownList>
    </div>
    <div style="position: absolute; visibility: hidden; top: 40px; left: 4px; width: 925px;
        height: 20px;" id="statusDiv">
        <asp:Label ID="Status" runat="server" Text="Ready"></asp:Label>
    </div>
    <div style="position: absolute; visibility: hidden; top: 88px; left: 5px; width: 174px;
        height: 548px;" id="browserDiv" class="frame">
        <asp:TreeView ID="BrowserTree" runat="server">
            <Nodes>
                <asp:TreeNode Text="New Node" Value="New Node"></asp:TreeNode>
            </Nodes>
        </asp:TreeView>
    </div>
    <div style="position: absolute; visibility: hidden; top: 87px; left: 191px; width: 739px;
        height: 330px; overflow: hidden" id="queryDiv" class="frame">
        <asp:TextBox ID="Query" runat="server" TextMode="MultiLine"></asp:TextBox>
        <script type="text/javascript">
            function InitEditor() {
                var myCodeMirror = CodeMirror.fromTextArea(document.getElementById("<%= Query.ClientID %>"), {
                    lineNumbers: true,
                    matchBrackets: true,
                    indentUnit: 4,
                    mode: "text/x-mysql"
                });
            }

            InitEditor();
        </script>
    </div>
    <div style="position: absolute; visibility: hidden; top: 427px; left: 190px; width: 733px;
        height: 210px;" id="resultsDiv" class="frame">
        <asp:Literal runat="server" ID="ResultsGrid"></asp:Literal>
    </div>
    </form>
</body>
</html>
