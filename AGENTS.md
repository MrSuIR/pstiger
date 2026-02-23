# AGENTS.md

## Project Overview

This repository contains interpreter for the Tiger programming language.

## Specification

Tiger programming language specification: `docs/specification/`.

## Coding Style

* Stack: C# 14 / .NET 10
* Dependencies: prefer .NET built-in API, but NuGet packages allowed if feature cannot be implemented with .NET built-in API.
* Code comments: Russian only, explain non-trivial logic and unobvious decisions.
* Exception messages: English only
* Exception handling: catch exception only to add more context or implement fallback strategies.

## Agent guidelines

* Use `dotnet build` to ensure no build errors and warnings in code
* Use `dotnet test` to run existing unit and integration acceptance tests
