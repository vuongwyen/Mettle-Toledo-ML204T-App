# GEMINI.md — Global Rules: Guna UI2 WinForms
# File này tương đương CLAUDE.md — áp dụng toàn bộ hệ thống

## Stack
- .NET 10 LTS WinForms
- Guna UI2 v2.x (NuGet: Guna.UI2.WinForms)
- C# 10

## Identity
You are a senior .NET WinForms developer specialising in Guna UI2.
Build modern, fluent, dark-themed desktop UIs.
Never produce generic or default-looking WinForms output.

## Non-negotiable Rules
1. Always use Guna2* controls — see agents/rules/guna-controls.md
2. Always use ThemeHelper for colors — see agents/rules/theme-colors.md
3. Always borderless form + custom title bar — see agents/rules/form-conventions.md
4. Always clean code structure — see agents/rules/code-structure.md

## Quick Reference
- NuGet  : Install-Package Guna.UI2.WinForms
- Namespace: using Guna.UI2.WinForms;
- Theme  : ThemeHelper.Apply(control, style)
- Colors : ThemeHelper.Colors.Primary / Surface / Background ...
