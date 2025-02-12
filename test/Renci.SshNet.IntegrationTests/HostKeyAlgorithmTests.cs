﻿using Renci.SshNet.Common;
using Renci.SshNet.IntegrationTests.Common;
using Renci.SshNet.TestTools.OpenSSH;

namespace Renci.SshNet.IntegrationTests
{
    [TestClass]
    public class HostKeyAlgorithmTests : IntegrationTestBase
    {
        private IConnectionInfoFactory _connectionInfoFactory;
        private RemoteSshdConfig _remoteSshdConfig;

        [TestInitialize]
        public void SetUp()
        {
            _connectionInfoFactory = new LinuxVMConnectionFactory(SshServerHostName, SshServerPort);
            _remoteSshdConfig = new RemoteSshd(new LinuxAdminConnectionFactory(SshServerHostName, SshServerPort)).OpenConfig();
        }

        [TestCleanup]
        public void TearDown()
        {
            _remoteSshdConfig?.Reset();
        }

        [TestMethod]
        public void SshDss()
        {
            DoTest(HostKeyAlgorithm.SshDss, HostKeyFile.Dsa);
        }

        [TestMethod]
        public void SshRsa()
        {
            DoTest(HostKeyAlgorithm.SshRsa, HostKeyFile.Rsa);
        }

        [TestMethod]
        public void SshRsaSha256()
        {
            DoTest(HostKeyAlgorithm.RsaSha2256, HostKeyFile.Rsa);
        }

        [TestMethod]
        public void SshRsaSha512()
        {
            DoTest(HostKeyAlgorithm.RsaSha2512, HostKeyFile.Rsa);
        }

        [TestMethod]
        public void SshEd25519()
        {
            DoTest(HostKeyAlgorithm.SshEd25519, HostKeyFile.Ed25519);
        }

        [TestMethod]
        public void Ecdsa256()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp256, HostKeyFile.Ecdsa256);
        }

        [TestMethod]
        public void Ecdsa384()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp384, HostKeyFile.Ecdsa384);
        }

        [TestMethod]
        public void Ecdsa521()
        {
            DoTest(HostKeyAlgorithm.EcdsaSha2Nistp521, HostKeyFile.Ecdsa521);
        }

        private void DoTest(HostKeyAlgorithm hostKeyAlgorithm, HostKeyFile hostKeyFile)
        {
            _remoteSshdConfig.ClearHostKeyAlgorithms()
                             .AddHostKeyAlgorithm(hostKeyAlgorithm)
                             .ClearHostKeyFiles()
                             .AddHostKeyFile(hostKeyFile.FilePath)
                             .Update()
                             .Restart();

            HostKeyEventArgs hostKeyEventsArgs = null;

            using (var client = new SshClient(_connectionInfoFactory.Create()))
            {
                client.HostKeyReceived += (sender, e) => hostKeyEventsArgs = e;
                client.Connect();
                client.Disconnect();
            }

            Assert.IsNotNull(hostKeyEventsArgs);
            Assert.AreEqual(hostKeyAlgorithm.Name, hostKeyEventsArgs.HostKeyName);
            Assert.AreEqual(hostKeyFile.KeyLength, hostKeyEventsArgs.KeyLength);
            CollectionAssert.AreEqual(hostKeyFile.FingerPrint, hostKeyEventsArgs.FingerPrint);
        }
    }
}
