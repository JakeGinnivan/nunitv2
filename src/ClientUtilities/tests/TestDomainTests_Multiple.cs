#region Copyright (c) 2003, James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole, Philip A. Craig
/************************************************************************************
'
' Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' Copyright  2000-2002 Philip A. Craig
'
' This software is provided 'as-is', without any express or implied warranty. In no 
' event will the authors be held liable for any damages arising from the use of this 
' software.
' 
' Permission is granted to anyone to use this software for any purpose, including 
' commercial applications, and to alter it and redistribute it freely, subject to the 
' following restrictions:
'
' 1. The origin of this software must not be misrepresented; you must not claim that 
' you wrote the original software. If you use this software in a product, an 
' acknowledgment (see the following) in the product documentation is required.
'
' Portions Copyright  2002-2003 James W. Newkirk, Michael C. Two, Alexei A. Vorontsov, Charlie Poole
' or Copyright  2000-2002 Philip A. Craig
'
' 2. Altered source versions must be plainly marked as such, and must not be 
' misrepresented as being the original software.
'
' 3. This notice may not be removed or altered from any source distribution.
'
'***********************************************************************************/
#endregion

using System;
using System.IO;
using System.Collections;
using NUnit.Framework;
using NUnit.Core;
using NUnit.Tests.Assemblies;

namespace NUnit.Util.Tests
{
	/// <summary>
	/// Summary description for MultipleAssembliesDomain.
	/// </summary>
	[TestFixture]
	public class TestDomainTests_Multiple
	{
		private string[] assemblies;
		private TestDomain domain; 
		private TextWriter outStream;
		private TextWriter errorStream;
		private Test loadedSuite;

		private string name = "Multiple Assemblies Test";

		[TestFixtureSetUp]
		public void Init()
		{
			outStream = new ConsoleWriter(Console.Out);
			errorStream = new ConsoleWriter(Console.Error);
			
			domain = new TestDomain();
			assemblies = new string[]
				{ Path.GetFullPath( "nonamespace-assembly.dll" ), Path.GetFullPath( "mock-assembly.dll" ) };
			loadedSuite = domain.Load( new TestProject( name, assemblies ) );
		}

		[TestFixtureTearDown]
		public void UnloadTestDomain()
		{
			domain.Unload();
			domain = null;
		}
			
		[Test]
		public void BuildSuite()
		{
			Assert.IsNotNull(loadedSuite);
		}

		[Test]
		public void RootNode()
		{
			Assert.IsTrue( loadedSuite is RootTestSuite );
			Assert.AreEqual( name, loadedSuite.Name );
		}

		[Test]
		public void AssemblyNodes()
		{
			Assert.IsTrue( loadedSuite.Tests[0] is TestAssembly );
			Assert.IsTrue( loadedSuite.Tests[1] is TestAssembly );
		}

		[Test]
		public void TestCaseCount()
		{
			Assert.AreEqual(NoNamespaceTestFixture.Tests + MockAssembly.Tests, 
				loadedSuite.CountTestCases());
		}

		[Test]
		public void RunMultipleAssemblies()
		{
			TestResult result = loadedSuite.Run(NullListener.NULL);
			ResultSummarizer summary = new ResultSummarizer(result);
			Assert.AreEqual(
				NoNamespaceTestFixture.Tests + MockAssembly.Tests - MockAssembly.NotRun, 
				summary.ResultCount);
		}
	}

	[TestFixture]
	public class TestDomainTests_MultipleFixture
	{
		[Test]
		public void LoadFixture()
		{
			string name = "Multiple Assemblies Test";
			string[] assemblies = new string[]
				{ Path.GetFullPath( "nonamespace-assembly.dll" ), Path.GetFullPath( "mock-assembly.dll" ) };

			TestDomain domain = new TestDomain();
			Test suite = domain.Load( new TestProject( name, assemblies ), "NUnit.Tests.Assemblies.MockTestFixture" );
			Assert.IsNotNull( suite );
			Assert.AreEqual( MockTestFixture.Tests, suite.CountTestCases() );
		}
	}
}
