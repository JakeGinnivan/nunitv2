// ****************************************************************
// This is free software licensed under the NUnit license. You
// may obtain a copy of the license as well as information regarding
// copyright ownership at http://nunit.org/?p=license&r=2.4.
// ****************************************************************

namespace NUnit.Core
{
	using System;
	using System.Collections;
	using System.Reflection;
	using NUnit.Core.Filters;

	/// <summary>
	/// Summary description for TestSuite.
	/// </summary>
	/// 
	[Serializable]
	public class TestSuite : Test
	{
		#region Fields
		/// <summary>
		/// Our collection of child tests
		/// </summary>
		private ArrayList tests = new ArrayList();

		/// <summary>
		/// The fixture setup method for this suite
		/// </summary>
		protected MethodInfo fixtureSetUp;

		/// <summary>
		/// The fixture teardown method for this suite
		/// </summary>
		protected MethodInfo fixtureTearDown;

		#endregion

		#region Constructors
		public TestSuite( string name ) 
			: base( name ) { }

		public TestSuite( string parentSuiteName, string name ) 
			: base( parentSuiteName, name ) { }

		public TestSuite( Type fixtureType )
			: base( fixtureType ) { }
		#endregion

		#region Public Methods
		public void Sort()
		{
			this.tests.Sort();

			foreach( Test test in Tests )
			{
				TestSuite suite = test as TestSuite;
				if ( suite != null )
					suite.Sort();
			}		
		}

		public void Sort(IComparer comparer)
		{
			this.tests.Sort(comparer);

			foreach( Test test in Tests )
			{
				TestSuite suite = test as TestSuite;
				if ( suite != null )
					suite.Sort(comparer);
			}
		}

		public void Add( Test test ) 
		{
//			if( test.RunState == RunState.Runnable )
//			{
//				test.RunState = this.RunState;
//				test.IgnoreReason = this.IgnoreReason;
//			}
			test.Parent = this;
			tests.Add(test);
		}

		public void Add( object fixture )
		{
			Test test = TestFixtureBuilder.BuildFrom( fixture );
			if ( test != null )
				Add( test );
		}
		#endregion

		#region Properties
		public override IList Tests 
		{
			get { return tests; }
		}

		public override bool IsSuite
		{
			get { return true; }
		}

		public override int TestCount
		{
			get
			{
				int count = 0;

				foreach(Test test in Tests)
				{
					count += test.TestCount;
				}
				return count;
			}
		}
		#endregion

		#region Test Overrides
		public override int CountTestCases(ITestFilter filter)
		{
			int count = 0;

			if(filter.Pass(this)) 
			{
				foreach(Test test in Tests)
				{
					count += test.CountTestCases(filter);
				}
			}
			return count;
		}

		public override TestResult Run(EventListener listener, ITestFilter filter)
		{
			using( new TestContext() )
			{
				TestResult suiteResult = new TestResult( this );

				listener.SuiteStarted( this.TestName );
				long startTime = DateTime.Now.Ticks;

				switch (this.RunState)
				{
					case RunState.Runnable:
					case RunState.Explicit:
                        suiteResult.Success(); // Assume success
                        DoOneTimeSetUp(suiteResult);
						if ( suiteResult.IsFailure || suiteResult.IsError )
							MarkTestsFailed(Tests, suiteResult, listener, filter);
						else
						{
							try
							{
								RunAllTests(suiteResult, listener, filter);
							}
							finally
							{
								DoOneTimeTearDown(suiteResult);
							}
						}
						break;

					default:
                    case RunState.Skipped:
				        SkipAllTests(suiteResult, listener, filter);
                        break;
                    case RunState.NotRunnable:
                        MarkAllTestsInvalid( suiteResult, listener, filter);
                        break;
                    case RunState.Ignored:
                        IgnoreAllTests(suiteResult, listener, filter);
                        break;
				}

				long stopTime = DateTime.Now.Ticks;
				double time = ((double)(stopTime - startTime)) / (double)TimeSpan.TicksPerSecond;
				suiteResult.Time = time;

				listener.SuiteFinished(suiteResult);
				return suiteResult;
			}
		}
		#endregion

		#region Virtual Methods
        protected virtual void DoOneTimeSetUp(TestResult suiteResult)
        {
            if (FixtureType != null)
            {
                try
                {
					// In case TestFixture was created with fixture object
					if (Fixture == null)
						CreateUserFixture();

                    if (this.Properties["_SETCULTURE"] != null)
                        TestContext.CurrentCulture =
                            new System.Globalization.CultureInfo((string)Properties["_SETCULTURE"]);

                    if (this.fixtureSetUp != null)
                        Reflect.InvokeMethod(fixtureSetUp, fixtureSetUp.IsStatic ? null : Fixture);
                }
                catch (Exception ex)
                {
                    if (ex is NUnitException || ex is System.Reflection.TargetInvocationException)
                        ex = ex.InnerException;

                    if (IsIgnoreException(ex))
                    {
                        this.RunState = RunState.Ignored;
                        suiteResult.Ignore(ex.Message);
                        suiteResult.StackTrace = ex.StackTrace;
                        this.IgnoreReason = ex.Message;
                    }
                    else 
                    {
                        if (IsAssertException(ex))
                            suiteResult.Failure(ex.Message, ex.StackTrace, FailureSite.SetUp);
                        else
                            suiteResult.Error(ex, FailureSite.SetUp);
                    }
                }
            }
        }

		protected virtual void CreateUserFixture()
		{
			Fixture = Reflect.Construct(FixtureType);
		}

        protected virtual void DoOneTimeTearDown(TestResult suiteResult)
        {
            if ( this.Fixture != null)
            {
                try
                {
                    if (this.fixtureTearDown != null)
                        Reflect.InvokeMethod(fixtureTearDown, fixtureTearDown.IsStatic ? null : Fixture);

					IDisposable disposable = Fixture as IDisposable;
					if (disposable != null)
						disposable.Dispose();
				}
                catch (Exception ex)
                {
					// Error in TestFixtureTearDown or Dispose causes the
					// suite to be marked as a failure, even if
					// all the contained tests passed.
					NUnitException nex = ex as NUnitException;
					if (nex != null)
						ex = nex.InnerException;


					suiteResult.Failure(ex.Message, ex.StackTrace, FailureSite.TearDown);
				}

                this.Fixture = null;
            }
        }

        protected virtual bool IsAssertException(Exception ex)
        {
            return ex.GetType().FullName == NUnitFramework.AssertException;
        }

        protected virtual bool IsIgnoreException(Exception ex)
        {
            return ex.GetType().FullName == NUnitFramework.IgnoreException;
        }
        
        #endregion

        #region Helper Methods

        private void RunAllTests(
			TestResult suiteResult, EventListener listener, ITestFilter filter )
		{
            foreach (Test test in ArrayList.Synchronized(Tests))
            {
                if (filter.Pass(test))
                {
                    RunState saveRunState = test.RunState;

                    if (test.RunState == RunState.Runnable && this.RunState != RunState.Runnable && this.RunState != RunState.Explicit )
                    {
                        test.RunState = this.RunState;
                        test.IgnoreReason = this.IgnoreReason;
                    }

                    TestResult result = test.Run(listener, filter);

                    suiteResult.AddResult(result);

                    if (saveRunState != test.RunState)
                    {
                        test.RunState = saveRunState;
                        test.IgnoreReason = null;
                    }
                }
            }
		}

        private void SkipAllTests(TestResult suiteResult, EventListener listener, ITestFilter filter)
        {
            suiteResult.Skip(this.IgnoreReason);
            MarkTestsNotRun(this.Tests, ResultState.Skipped, this.IgnoreReason, suiteResult, listener, filter);
        }

        private void IgnoreAllTests(TestResult suiteResult, EventListener listener, ITestFilter filter)
        {
            suiteResult.Ignore(this.IgnoreReason);
            MarkTestsNotRun(this.Tests, ResultState.Ignored, this.IgnoreReason, suiteResult, listener, filter);
        }

        private void MarkAllTestsInvalid(TestResult suiteResult, EventListener listener, ITestFilter filter)
        {
            suiteResult.Invalid(this.IgnoreReason);
            MarkTestsNotRun(this.Tests, ResultState.NotRunnable, this.IgnoreReason, suiteResult, listener, filter);
        }
       
        private void MarkTestsNotRun(
            IList tests, ResultState resultState, string ignoreReason, TestResult suiteResult, EventListener listener, ITestFilter filter)
        {
            foreach (Test test in ArrayList.Synchronized(tests))
            {
                if (filter.Pass(test))
                    MarkTestNotRun(test, resultState, ignoreReason, suiteResult, listener, filter);
            }
        }

        private void MarkTestNotRun(
            Test test, ResultState resultState, string ignoreReason, TestResult suiteResult, EventListener listener, ITestFilter filter)
        {
            if (test is TestSuite)
            {
                listener.SuiteStarted(test.TestName);
                TestResult result = new TestResult( new TestInfo(test) );
				result.SetResult( resultState, ignoreReason, null );
                MarkTestsNotRun(test.Tests, resultState, ignoreReason, suiteResult, listener, filter);
                suiteResult.AddResult(result);
                listener.SuiteFinished(result);
            }
            else
            {
                listener.TestStarted(test.TestName);
                TestResult result = new TestResult( new TestInfo(test) );
                result.SetResult( resultState, ignoreReason, null );
                suiteResult.AddResult(result);
                listener.TestFinished(result);
            }
        }

        private void MarkTestsFailed(
            IList tests, TestResult suiteResult, EventListener listener, ITestFilter filter)
        {
            foreach (Test test in ArrayList.Synchronized(tests))
                if (filter.Pass(test))
                    MarkTestFailed(test, suiteResult, listener, filter);
        }

        private void MarkTestFailed(
            Test test, TestResult suiteResult, EventListener listener, ITestFilter filter)
        {
            if (test is TestSuite)
            {
                listener.SuiteStarted(test.TestName);
                TestResult result = new TestResult( new TestInfo(test) );
				string msg = string.Format( "Parent SetUp failed in {0}", this.FixtureType.Name );
				result.Failure(msg, null, FailureSite.Parent);
                MarkTestsFailed(test.Tests, suiteResult, listener, filter);
                suiteResult.AddResult(result);
                listener.SuiteFinished(result);
            }
            else
            {
                listener.TestStarted(test.TestName);
                TestResult result = new TestResult( new TestInfo(test) );
				string msg = string.Format( "TestFixtureSetUp failed in {0}", this.FixtureType.Name );
				result.Failure(msg, null, FailureSite.Parent);
				suiteResult.AddResult(result);
                listener.TestFinished(result);
            }
        }
        #endregion
    }
}
