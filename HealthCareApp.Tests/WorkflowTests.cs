using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace HealthCareApp.Tests
{
    public class WorkflowTests
    {
        [Fact]
        public void Test_FailingTestShouldFailWorkflow()
        {
            Assert.Fail("This tests the CI-pipeline to make sure a pull request with failing tests don't go through.");
        }
    }
}