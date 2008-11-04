// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace NUnit.Core
{
    public class TestProcessInfo
    {
        private string moduleName;
        private int processId;
        private RuntimeFramework runtimeFramework;

        public TestProcessInfo( TestRunner runner )
        {
        }

        public int ProcessId
        {
            get { return processId; }
        }

        public string ModuleName
        {
            get { return moduleName; }
        }

        public RuntimeFramework RuntimeFramework
        {
            get { return runtimeFramework; }
        }
    }

	/// <summary>
	/// TestAssemblyInfo holds information about a loaded test assembly
	/// </summary>
	[Serializable]
	public class TestAssemblyInfo
	{
		private string assemblyName;
        private Version imageRuntimeVersion;
        private RuntimeFramework runnerRuntimeFramework;
        private int processId;
        private string moduleName;
        private string domainName;
        private IList testFrameworks;

        /// <summary>
        /// Constructs a TestAssemblyInfo
        /// </summary>
        /// <param name="assemblyName">The name of the assembly</param>
        /// <param name="imageRuntimeVersion">The version of the runtime for which the assembly was built</param>
        /// <param name="runnerRuntimeFramework">The runtime framework under which the assembly is loaded</param>
        /// <param name="testFrameworks">A list of test framework useds by the assembly</param>
		public TestAssemblyInfo( string assemblyName, Version imageRuntimeVersion, RuntimeFramework runnerRuntimeFramework, IList testFrameworks )
		{
			this.assemblyName = assemblyName;
            this.imageRuntimeVersion = imageRuntimeVersion;
            this.runnerRuntimeFramework = runnerRuntimeFramework;
            this.testFrameworks = testFrameworks;
            Process p = Process.GetCurrentProcess();
            this.processId = p.Id;
            this.moduleName = p.MainModule.ModuleName;
            this.domainName = AppDomain.CurrentDomain.FriendlyName;
		}

        /// <summary>
        /// Gets the name of the assembly
        /// </summary>
		public string Name
		{
			get { return assemblyName; }
		}

        /// <summary>
        /// Gets the runtime version for which the assembly was built
        /// </summary>
        public Version ImageRuntimeVersion
        {
            get { return imageRuntimeVersion; }
        }

        /// <summary>
        /// Gets the runtime framework under which the assembly is loaded
        /// </summary>
        public RuntimeFramework RunnerRuntimeFramework
        {
            get { return runnerRuntimeFramework; }
        }

        /// <summary>
        /// Gets the runtime version under which the assembly is loaded
        /// </summary>
        public Version RunnerRuntimeVersion
        {
            get { return runnerRuntimeFramework.Version; }
        }

        public int ProcessId
        {
            get { return processId; }
        }

        public string DomainName
        {
            get { return domainName; }
        }

        public string ModuleName
        {
            get { return moduleName; }
        }

        /// <summary>
        /// Gets a list of testframeworks referenced by the assembly
        /// </summary>
		public IList TestFrameworks
		{
			get { return testFrameworks; }
		}
    }
}
