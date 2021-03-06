﻿using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoSharp.VM.Extensions;
using NeoSharp.VM.Interop.Tests.Crypto;
using NeoSharp.VM.Interop.Tests.Extra;
using NeoSharp.VM.Interop.Types.StackItems;

namespace NeoSharp.VM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_CRYPTO : VMOpCodeTest
    {
        class Verify
        {
            public readonly byte[] Message;
            public readonly byte[] Signature;
            public readonly byte[] PublicKey;

            public Verify(string msg, string sig, string pk)
            {
                Message = msg.FromHexString();
                Signature = sig.FromHexString();
                PublicKey = pk.FromHexString();
            }
        }

        [TestMethod]
        public void SHA1()
        {
            InternalTestBigInteger(EVMOpCode.SHA1, (a) =>
            {
                using (var sha = System.Security.Cryptography.SHA1.Create())
                {
                    return sha.ComputeHash(a == 0 ? new byte[] { } : a.ToByteArray());
                }
            });
        }

        [TestMethod]
        public void SHA256()
        {
            InternalTestBigInteger(EVMOpCode.SHA256, (a) =>
            {
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    return sha.ComputeHash(a == 0 ? new byte[] { } : a.ToByteArray());
                }
            });
        }

        [TestMethod]
        public void HASH160()
        {
            InternalTestBigInteger(EVMOpCode.HASH160, (a) =>
            {
                using (var sha = System.Security.Cryptography.SHA256.Create())
                using (var ripe = new RIPEMD160Managed())
                {
                    var hash = sha.ComputeHash(a == 0 ? new byte[] { } : a.ToByteArray());
                    return ripe.ComputeHash(hash);
                }
            });
        }

        [TestMethod]
        public void HASH256()
        {
            InternalTestBigInteger(EVMOpCode.HASH256, (a) =>
            {
                using (var sha = System.Security.Cryptography.SHA256.Create())
                {
                    var hash = sha.ComputeHash(a == 0 ? new byte[] { } : a.ToByteArray());
                    return sha.ComputeHash(hash);
                }
            });
        }

        [TestMethod]
        public void CHECKSIG()
        {
            // Without push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.CHECKSIG
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                using (var currentContext = engine.CurrentContext)
                {
                    using (var i = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(i.Value, 0x05);
                    }
                }

                CheckClean(engine, false);
            }

            // Without get message

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(new ExecutionEngineArgs()))
            {
                // Load script

                // signature

                script.EmitPush(new byte[] {
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02 });

                // publicKey

                script.EmitPush(new byte[] {
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02, 0x03, 0x04, 0x00, 0x01, 0x02, 0x03, 0x04,
                    0x00, 0x01, 0x02 });

                script.Emit(EVMOpCode.CHECKSIG);

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var i = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(i.Value);
                }

                CheckClean(engine, false);
            }

            // Without valid push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH6,
                EVMOpCode.CHECKSIG
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var i = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(i.Value);
                }

                CheckClean(engine, false);
            }

            // Real message

            foreach (bool ok in new bool[] { true, false })
                foreach (var ver in new Verify[]
                {
                new Verify(
                    "00000000ea5029691bd94d9667cb32bf136cbba38cf9eb5978bd1d0bf825a3f8a80be6af157aee574e343ff867f3c470ffeecd77312bed61195ba8f1c6588fd275257f60ef6b0458d6070000a36a49f800ef916159e75d652b5d3827bf04c165bbe9ef95cca4bf55",
                    "95083c5c98cdacdaf57af61104b68940cd0f7cae59b907ddea7f77ae1c4884348321ab62e65eabd82876e2e5f58f822538633521307be831a260ecab2cc5d16c",
                    "03b8d9d5771d8f513aa0869b9cc8d50986403b78c6da36890638c3d46a5adce04a"),
                new Verify(
                    "00000000bf4421c88776c53b43ce1dc45463bfd2028e322fdfb60064be150ed3e36125d418f98ec3ed2c2d1c9427385e7b85d0d1a366e29c4e399693a59718380f8bbad6d6d90358010000004490d0bb7170726c59e75d652b5d3827bf04c165bbe9ef95cca4bf55",
                    "4e0ebd369e81093866fe29406dbf6b402c003774541799d08bf9bb0fc6070ec0f6bad908ab95f05fa64e682b485800b3c12102a8596e6c715ec76f4564d5eff3",
                    "ca0e27697b9c248f6f16e085fd0061e26f44da85b58ee835c110caa5ec3ba5543672835e89a5c1f821d773214881e84618770508ce1ddfd488ae377addf7ca38")
                })

                    using (var script = new ScriptBuilder())
                    using (var engine = CreateEngine(new ExecutionEngineArgs()
                    {
                        MessageProvider = new DummyMessageProvider(0, ver.Message)
                    }))
                    {
                        // Signature

                        script.EmitPush(ver.Signature);

                        // PublicKey

                        if (!ok)
                        {
                            byte[] bc = ver.PublicKey.Take(ver.PublicKey.Length - 1)
                                .Concat(new byte[] { (byte)(ver.PublicKey[ver.PublicKey.Length - 1] + 1) }).ToArray();

                            script.EmitPush(bc);
                        }
                        else
                        {
                            script.EmitPush(ver.PublicKey);
                        }

                        script.Emit(EVMOpCode.CHECKSIG);

                        // Load script

                        engine.LoadScript(script);

                        // Execute

                        Assert.IsTrue(engine.Execute());

                        // Check

                        using (var i = engine.ResultStack.Pop<BooleanStackItem>())
                        {
                            Assert.AreEqual(ok, i.Value);
                        }

                        CheckClean(engine, false);
                    }
        }

        [TestMethod]
        public void VERIFY()
        {
            // Without push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.VERIFY
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                using (var currentContext = engine.CurrentContext)
                {
                    using (var i = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(i.Value, 0x05);
                    }
                }

                CheckClean(engine, false);
            }

            // Without valid push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.PUSH6,
                EVMOpCode.PUSH6,
                EVMOpCode.VERIFY
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsTrue(engine.Execute());

                // Check

                using (var i = engine.ResultStack.Pop<BooleanStackItem>())
                {
                    Assert.IsFalse(i.Value);
                }

                CheckClean(engine, false);
            }

            // Real message

            foreach (bool ok in new bool[] { true, false })
                foreach (var ver in new Verify[]
                {
                new Verify(
                    "00000000ea5029691bd94d9667cb32bf136cbba38cf9eb5978bd1d0bf825a3f8a80be6af157aee574e343ff867f3c470ffeecd77312bed61195ba8f1c6588fd275257f60ef6b0458d6070000a36a49f800ef916159e75d652b5d3827bf04c165bbe9ef95cca4bf55",
                    "95083c5c98cdacdaf57af61104b68940cd0f7cae59b907ddea7f77ae1c4884348321ab62e65eabd82876e2e5f58f822538633521307be831a260ecab2cc5d16c",
                    "03b8d9d5771d8f513aa0869b9cc8d50986403b78c6da36890638c3d46a5adce04a"),
                new Verify(
                    "00000000bf4421c88776c53b43ce1dc45463bfd2028e322fdfb60064be150ed3e36125d418f98ec3ed2c2d1c9427385e7b85d0d1a366e29c4e399693a59718380f8bbad6d6d90358010000004490d0bb7170726c59e75d652b5d3827bf04c165bbe9ef95cca4bf55",
                    "4e0ebd369e81093866fe29406dbf6b402c003774541799d08bf9bb0fc6070ec0f6bad908ab95f05fa64e682b485800b3c12102a8596e6c715ec76f4564d5eff3",
                    "ca0e27697b9c248f6f16e085fd0061e26f44da85b58ee835c110caa5ec3ba5543672835e89a5c1f821d773214881e84618770508ce1ddfd488ae377addf7ca38")
                })

                    using (var script = new ScriptBuilder())
                    using (var engine = CreateEngine(new ExecutionEngineArgs()
                    {
                        MessageProvider = null
                    }))
                    {
                        // Message

                        script.EmitPush(ver.Message);

                        // Signature

                        script.EmitPush(ver.Signature);

                        // PublicKey

                        if (!ok)
                        {
                            byte[] bc = ver.PublicKey.Take(ver.PublicKey.Length - 1)
                                .Concat(new byte[] { (byte)(ver.PublicKey[ver.PublicKey.Length - 1] + 1) }).ToArray();

                            script.EmitPush(bc);
                        }
                        else
                        {
                            script.EmitPush(ver.PublicKey);
                        }

                        script.Emit(EVMOpCode.VERIFY);

                        // Load script

                        engine.LoadScript(script);

                        // Execute

                        Assert.IsTrue(engine.Execute());

                        // Check

                        using (var i = engine.ResultStack.Pop<BooleanStackItem>())
                        {
                            Assert.AreEqual(ok, i.Value);
                        }

                        CheckClean(engine, false);
                    }
        }

        [TestMethod]
        public void CHECKMULTISIG()
        {
            // Without push

            using (var script = new ScriptBuilder
                (
                EVMOpCode.PUSH5,
                EVMOpCode.CHECKMULTISIG
                ))
            using (var engine = CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                using (var currentContext = engine.CurrentContext)
                {
                    using (var i = currentContext.EvaluationStack.Pop<IntegerStackItem>())
                    {
                        Assert.AreEqual(i.Value, 0x05);
                    }
                }

                CheckClean(engine, false);
            }

            // Check fault by wrong type

            foreach (bool inSignature in new bool[] { true, false })
                foreach (bool inArray in new bool[] { true, false })
                    using (var script = new ScriptBuilder())
                    using (var engine = CreateEngine(new ExecutionEngineArgs()
                    {
                        MessageProvider = new DummyMessageProvider()
                    }))
                    {
                        // Signature

                        if (inSignature)
                        {
                            if (inArray)
                            {
                                script.Emit(EVMOpCode.NEWMAP);
                                script.Emit(EVMOpCode.PUSH1);
                                script.Emit(EVMOpCode.PACK);
                            }
                            else
                            {
                                script.Emit(EVMOpCode.NEWMAP);
                                script.Emit(EVMOpCode.PUSH1);
                            }
                        }
                        else
                        {
                            script.EmitPush(new byte[32]);
                            script.Emit(EVMOpCode.PUSH1);
                        }

                        // PublicKey

                        if (!inSignature)
                        {
                            if (inArray)
                            {
                                script.Emit(EVMOpCode.NEWMAP);
                                script.Emit(EVMOpCode.PUSH1);
                                script.Emit(EVMOpCode.PACK);
                            }
                            else
                            {
                                script.Emit(EVMOpCode.NEWMAP);
                                script.Emit(EVMOpCode.PUSH1);
                            }
                        }
                        else
                        {
                            script.EmitPush(new byte[32]);
                            script.Emit(EVMOpCode.PUSH1);
                        }

                        script.Emit(EVMOpCode.CHECKMULTISIG);

                        // Load script

                        engine.LoadScript(script);

                        // Execute

                        Assert.IsFalse(engine.Execute());

                        // Check

                        CheckClean(engine, false);
                    }

            // Check fault by empty array

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(new ExecutionEngineArgs()
            {
                MessageProvider = new DummyMessageProvider()
            }))
            {
                // Signature

                script.Emit(EVMOpCode.PUSH0);
                script.Emit(EVMOpCode.NEWARRAY);

                // PublicKey

                script.EmitPush(new byte[32]);
                script.EmitPush(1);

                script.Emit(EVMOpCode.CHECKMULTISIG);

                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Check fault by more less push

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(new ExecutionEngineArgs()
            {
                MessageProvider = new DummyMessageProvider()
            }))
            {
                // Signature

                script.EmitPush(new byte[32]);
                script.EmitPush(2);

                // PublicKey

                script.EmitPush(new byte[32]);
                script.EmitPush(1);

                script.Emit(EVMOpCode.CHECKMULTISIG);

                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                using (var currentContext = engine.CurrentContext)
                {
                    using (var i = currentContext.EvaluationStack.Pop<ByteArrayStackItem>())
                    {
                        // Nothing to do
                    }
                }

                CheckClean(engine, false);
            }

            // Check fault by more signatures

            using (var script = new ScriptBuilder())
            using (var engine = CreateEngine(new ExecutionEngineArgs()
            {
                MessageProvider = new DummyMessageProvider()
            }))
            {
                // Signature

                script.EmitPush(new byte[32]);
                script.EmitPush(new byte[32]);
                script.EmitPush(2);

                // PublicKey

                script.EmitPush(new byte[32]);
                script.EmitPush(1);

                script.Emit(EVMOpCode.CHECKMULTISIG);

                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.IsFalse(engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real message (array/int)

            foreach (bool withMessage in new bool[] { true, false })
                foreach (bool isArray in new bool[] { true, false })
                    foreach (bool ok in new bool[] { true, false })
                        foreach (var ver in new Verify[]
                        {
                    new Verify(
                        "00000000ea5029691bd94d9667cb32bf136cbba38cf9eb5978bd1d0bf825a3f8a80be6af157aee574e343ff867f3c470ffeecd77312bed61195ba8f1c6588fd275257f60ef6b0458d6070000a36a49f800ef916159e75d652b5d3827bf04c165bbe9ef95cca4bf55",
                        "95083c5c98cdacdaf57af61104b68940cd0f7cae59b907ddea7f77ae1c4884348321ab62e65eabd82876e2e5f58f822538633521307be831a260ecab2cc5d16c",
                        "03b8d9d5771d8f513aa0869b9cc8d50986403b78c6da36890638c3d46a5adce04a"),
                    new Verify(
                        "00000000bf4421c88776c53b43ce1dc45463bfd2028e322fdfb60064be150ed3e36125d418f98ec3ed2c2d1c9427385e7b85d0d1a366e29c4e399693a59718380f8bbad6d6d90358010000004490d0bb7170726c59e75d652b5d3827bf04c165bbe9ef95cca4bf55",
                        "4e0ebd369e81093866fe29406dbf6b402c003774541799d08bf9bb0fc6070ec0f6bad908ab95f05fa64e682b485800b3c12102a8596e6c715ec76f4564d5eff3",
                        "ca0e27697b9c248f6f16e085fd0061e26f44da85b58ee835c110caa5ec3ba5543672835e89a5c1f821d773214881e84618770508ce1ddfd488ae377addf7ca38")
                        })

                            using (var script = new ScriptBuilder())
                            using (var engine = CreateEngine(new ExecutionEngineArgs()
                            {
                                MessageProvider = withMessage ? new DummyMessageProvider(0, ver.Message) : null
                            }))
                            {
                                // Signature

                                script.EmitPush(ver.Signature);
                                script.EmitPush(1);
                                if (isArray) script.Emit(EVMOpCode.PACK);

                                // PublicKey

                                if (!ok)
                                {
                                    byte[] bc = ver.PublicKey.Take(ver.PublicKey.Length - 1)
                                        .Concat(new byte[] { (byte)(ver.PublicKey[ver.PublicKey.Length - 1] + 1) }).ToArray();

                                    script.EmitPush(bc);
                                }
                                else
                                {
                                    script.EmitPush(ver.PublicKey);
                                }

                                script.EmitPush(1);
                                if (isArray) script.Emit(EVMOpCode.PACK);

                                script.Emit(EVMOpCode.CHECKMULTISIG);

                                // Load script

                                engine.LoadScript(script);

                                // Execute

                                Assert.IsTrue(engine.Execute());

                                // Check

                                using (var i = engine.ResultStack.Pop<BooleanStackItem>())
                                {
                                    Assert.AreEqual(ok && withMessage, i.Value);
                                }

                                CheckClean(engine, false);
                            }
        }
    }
}