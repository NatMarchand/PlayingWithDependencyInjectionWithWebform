﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Main.Master" CodeBehind="Index.aspx.cs" Inherits="PlayingWithDependencyInjectionInWebForm.AutofacImplementation.Index" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeaderPlaceHolder" runat="server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder" runat="server">
    <h1>Hi from Index</h1>
    <div><%=Dependency.GetFormattedTime() %></div>
</asp:Content>
