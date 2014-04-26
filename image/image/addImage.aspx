<%@ Page Title="添加图片" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="addImage.aspx.cs" Inherits="image.addImage" %>
<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="FeaturedContent" runat="server">
        <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1><%: Title %></h1>
            </hgroup>
            <asp:FileUpload ID="FileUpload1" runat="server" />
            <asp:Button ID="Button2" runat="server" OnClick="Button2_Click" Text="上传" />
            <asp:Panel ID="Panel1" runat="server">
                <asp:Label ID="Label1" runat="server" Text="文物名："></asp:Label>
                <asp:TextBox ID="name_box" runat="server"></asp:TextBox>
                <br />
                <asp:Label ID="Label2" runat="server" Text="类别"></asp:Label>
                <asp:TextBox ID="category_box" runat="server"></asp:TextBox>
                <br />
                <asp:Label ID="Label3" runat="server" Text="年代"></asp:Label>
                <asp:TextBox ID="period_box" runat="server"></asp:TextBox>
                <br />
                <asp:Label ID="Label4" runat="server" Text="存放地点"></asp:Label>
                <asp:TextBox ID="museum_box" runat="server"></asp:TextBox>
                <br />
                <asp:Label ID="Label5" runat="server" Text="介绍："></asp:Label>
                <asp:TextBox ID="intro_box" runat="server"></asp:TextBox>
                <asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="保存" />
                <asp:Image ID="Image1" runat="server" Width="100px" />
            </asp:Panel>
        </div>
    </section>
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainContent" runat="server">
</asp:Content>
