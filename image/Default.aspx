<%@ Page Title="主页" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="image._Default" %>

<asp:Content runat="server" ID="FeaturedContent" ContentPlaceHolderID="FeaturedContent">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %>.</h1>
                <h2>主页</h2>
            </hgroup>
        </div>
    </section>
</asp:Content>
<asp:Content runat="server" ID="BodyContent" ContentPlaceHolderID="MainContent">
        <asp:Panel ID="Panel1" runat="server" Width="400px" CssClass="float-left">
            <h3>
        <asp:CheckBoxList ID="CheckBoxList1" runat="server" OnSelectedIndexChanged="CheckBoxList1_SelectedIndexChanged">
            <asp:ListItem Value="color">按照颜色查找</asp:ListItem>
            <asp:ListItem Value="size">按照形状查找</asp:ListItem>
            <asp:ListItem Value="grain">按照纹理查找</asp:ListItem>
        </asp:CheckBoxList>
</h3>

    <h3>
        <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="查找" />
        <asp:FileUpload ID="FileUpload1" runat="server" />
        <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="上传" />
        
</h3>
            
    </asp:Panel>
    <asp:Panel ID="Panel2" runat="server" CssClass="float-right">
        <asp:ListView ID="ListView1" runat="server">

        </asp:ListView>
        </asp:Panel>
</asp:Content>
