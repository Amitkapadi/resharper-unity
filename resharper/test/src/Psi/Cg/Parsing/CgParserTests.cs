﻿using JetBrains.ReSharper.Plugins.Unity.Cg.ProjectModel;
using JetBrains.ReSharper.Plugins.Unity.Cg.Psi;
using JetBrains.ReSharper.TestFramework;
using NUnit.Framework;

namespace JetBrains.ReSharper.Plugins.Unity.Tests.Psi.Cg.Parsing
{
    [TestUnity]
    [TestFileExtension(CgProjectFileType.CG_EXTENSION)]
    public class CgParserTests : ParserTestBase<CgLanguage>
    {
        protected override string RelativeTestDataPath => @"psi\cg\parsing";
        
        [TestCase("AllDeclarations")]
        
        [TestCase("Comment")]
        
        [TestCase("Empty")]
        
        [TestCase("FieldDeclaration")]
        [TestCase("FieldDeclarationNoSemicolon")]
        [TestCase("FieldDeclarationScalarTypes")]
        [TestCase("FieldDeclarationReferencedType")]
        [TestCase("FieldDeclarationWithInitializer")]
        
        [TestCase("FunctionBody")]
        
        [TestCase("FunctionBuiltInReturnType")]
        [TestCase("FunctionOneArgument")]
        [TestCase("FunctionTwoArguments")]
        [TestCase("FunctionVoidReturnType")]
        
        [TestCase("Semantics")]
        
        [TestCase("StructDeclaration")]
        [TestCase("StructDeclarationNoSemicolon")]
        [TestCase("StructDeclarationReferencedType")]
        public void TestParser(string name) => DoOneTest(name);
    }
}