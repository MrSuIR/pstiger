#!/usr/bin/env powershell
#
# Использует ildasm для дизассемблирования исполняемых файлов .NET в текстовое представление MSIL.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Print-Usage
{
    $ScriptName = Split-Path -Path $MyInvocation.ScriptName -Leaf
    Write-Host "Usage: $ScriptName <program.exe>"
    Write-Host "Uses ildasm to decompile .NET program into MSIL"
}

if ($args.Count -ne 1)
{
    Write-Error "Error: missing arguments"
    Print-Usage
    exit 1
}

if ($args[0] -in '-h', '--help')
{
    Print-Usage
    exit 0
}

$InputFile = $args[0]
$OutputFile = [System.IO.Path]::ChangeExtension($InputFile, '.il')

ildasm "$InputFile" -utf8 "-out=$OutputFile"
if ($LASTEXITCODE -ne 0)
{
    throw "ildasm failed with exit code $LASTEXITCODE"
}

Write-Host "Disassembled OK, see $OutputFile"