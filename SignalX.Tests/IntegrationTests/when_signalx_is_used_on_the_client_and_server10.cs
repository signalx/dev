namespace SignalXLib.Tests
{
    using SignalXLib.TestHelperLib;
    using Xunit;

    public class when_signalx_is_used_on_the_client_and_server10
    {

        [Fact]
        public void DIRTY_SANITY_TEST()

        {
            TestObject testObject = SignalXTester.SetupGeneralTest();

            Assert.NotEqual(testObject.Message, testObject.FinalMessage);
            Assert.NotEqual(testObject.Message, testObject.FinalMessage2);
            Assert.NotEqual(testObject.Message, testObject.FinalMessage3);
            Assert.NotEqual(testObject.Message, testObject.FinalMessage4);

            //ASSERT
            SignalXTester.CheckExpectations(
                () =>
                {
                    Assert.Equal(testObject.Message, testObject.FinalMessage);
                    Assert.Equal(testObject.Message, testObject.FinalMessage2);
                    Assert.Equal(testObject.Message, testObject.FinalMessage3);
                    Assert.Equal(testObject.Message, testObject.FinalMessage4);
                    Assert.True(testObject.VerifiedJoinedGroup, "verifiedJoinedGroup");
                    Assert.True(testObject.VerifiedJoinedGroup2, "verifiedJoinedGroup2");
                }, "http://localhost:" + SignalXTester.FreeTcpPort(), testObject);
        }
    }
}