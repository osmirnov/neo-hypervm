﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using NeoVM.Interop.Enums;
using NeoVM.Interop.Types;
using NeoVM.Interop.Types.StackItems;

namespace NeoVM.Interop.Tests
{
    [TestClass]
    public class VMOpCode_ARRAY : VMOpCodeTest
    {
        [TestMethod]
        public void ARRAYSIZE()
        {
            // With wrong type

            using (ScriptBuilder script = new ScriptBuilder())
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                script.EmitSysCall("System.ExecutionEngine.GetScriptContainer");
                script.Emit(EVMOpCode.ARRAYSIZE);
                script.Emit(EVMOpCode.RET);

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Without push

            using (ScriptBuilder script = new ScriptBuilder(EVMOpCode.ARRAYSIZE))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (ScriptBuilder script = new ScriptBuilder
                (
                    EVMOpCode.PUSH3,
                    EVMOpCode.NEWARRAY,
                    EVMOpCode.ARRAYSIZE,

                    EVMOpCode.PUSHBYTES1, 0x00,
                    EVMOpCode.ARRAYSIZE,

                    EVMOpCode.RET
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 3);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PACK()
        {
            using (ScriptBuilder script = new ScriptBuilder
                (
                    EVMOpCode.PUSH5,
                    EVMOpCode.PUSH6,
                    EVMOpCode.PUSH2,
                    EVMOpCode.PACK,
                    EVMOpCode.RET
                ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                using (ArrayStackItem arr = engine.EvaluationStack.Peek<ArrayStackItem>(0))
                {
                    Assert.IsTrue(arr != null);
                    Assert.IsTrue(arr[0] is IntegerStackItem b1 && b1.Value == 0x06);
                    Assert.IsTrue(arr[1] is IntegerStackItem b2 && b2.Value == 0x05);
                }

                // Remove array and test clean

                engine.EvaluationStack.Pop();

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void UNPACK()
        {
            // Without array

            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH10,
                       EVMOpCode.UNPACK,
                       EVMOpCode.RET
                   ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Without push

            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.UNPACK,
                       EVMOpCode.RET
                   ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real tests

            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH5,
                       EVMOpCode.PUSH6,
                       EVMOpCode.PUSH2,
                       EVMOpCode.PACK,
                       EVMOpCode.UNPACK,
                       EVMOpCode.RET
                   ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 0x02);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 0x06);
                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 0x05);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void PICKITEM_ARRAY()
        {
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH1,
                       EVMOpCode.PUSH2,
                       EVMOpCode.PUSH3,
                       EVMOpCode.PUSH3,
                       EVMOpCode.PACK,
                       EVMOpCode.PUSH2,
                       EVMOpCode.PICKITEM,
                       EVMOpCode.RET
                   ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop<IntegerStackItem>().Value == 1);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SETITEM_ARRAY()
        {
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH1,
                       EVMOpCode.NEWARRAY,
                       EVMOpCode.DUP,
                       EVMOpCode.PUSH0,
                       EVMOpCode.PUSH5,
                       EVMOpCode.SETITEM,
                       EVMOpCode.RET
                   ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Count == 1);

                using (ArrayStackItem arr = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(arr != null && !arr.IsStruct);
                    Assert.IsTrue(arr.Count == 1);
                    Assert.IsTrue(arr[0] is IntegerStackItem b0 && b0.Value == 5);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void SETITEM_STRUCT()
        {
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.PUSH1,
                       EVMOpCode.NEWSTRUCT,
                       EVMOpCode.DUP,
                       EVMOpCode.PUSH0,
                       EVMOpCode.PUSH5,
                       EVMOpCode.SETITEM,
                       EVMOpCode.RET
                   ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Count == 1);

                using (ArrayStackItem arr = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(arr != null && arr.IsStruct);
                    Assert.IsTrue(arr.Count == 1);
                    Assert.IsTrue(arr[0] is IntegerStackItem b0 && b0.Value == 5);
                }

                CheckClean(engine);
            }
        }

        void NEWARRAY_NEWSTRUCT(bool isStruct)
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder
            (
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // With push (-1)

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSHM1,
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (ScriptBuilder script = new ScriptBuilder
            (
                EVMOpCode.PUSH2,
                isStruct ? EVMOpCode.NEWSTRUCT : EVMOpCode.NEWARRAY,
                EVMOpCode.RET
            ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                using (ArrayStackItem arr = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(arr != null && arr.IsStruct == isStruct);
                    Assert.IsTrue(arr.Count == 2);
                    Assert.IsTrue(arr[0] is BooleanStackItem b0 && !b0.Value);
                    Assert.IsTrue(arr[1] is BooleanStackItem b1 && !b1.Value);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void NEWARRAY() { NEWARRAY_NEWSTRUCT(false); }

        [TestMethod]
        public void NEWSTRUCT() { NEWARRAY_NEWSTRUCT(true); }

        [TestMethod]
        public void NEWMAP()
        {
            using (ScriptBuilder script = new ScriptBuilder
                   (
                       EVMOpCode.NEWMAP,
                       EVMOpCode.RET
                   ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                Assert.IsTrue(engine.EvaluationStack.Pop() is MapStackItem);

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void APPEND()
        {
            Assert.IsFalse(true);
        }

        [TestMethod]
        public void REVERSE()
        {
            // Without push

            using (ScriptBuilder script = new ScriptBuilder(EVMOpCode.REVERSE))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Without Array

            using (ScriptBuilder script = new ScriptBuilder
                    (
                        EVMOpCode.PUSH9,
                        EVMOpCode.REVERSE
                    ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.FAULT, engine.Execute());

                // Check

                CheckClean(engine, false);
            }

            // Real test

            using (ScriptBuilder script = new ScriptBuilder
                    (
                        EVMOpCode.PUSH9,
                        EVMOpCode.PUSH8,
                        EVMOpCode.PUSH2,
                        EVMOpCode.PACK,
                        EVMOpCode.DUP,
                        EVMOpCode.REVERSE
                    ))
            using (ExecutionEngine engine = NeoVM.CreateEngine(Args))
            {
                // Load script

                engine.LoadScript(script);

                // Execute

                Assert.AreEqual(EVMState.HALT, engine.Execute());

                // Check

                using (ArrayStackItem ar = engine.EvaluationStack.Pop<ArrayStackItem>())
                {
                    Assert.IsTrue(ar[0] is IntegerStackItem i0 && i0.Value == 0x09);
                    Assert.IsTrue(ar[1] is IntegerStackItem i1 && i1.Value == 0x08);
                }

                CheckClean(engine);
            }
        }

        [TestMethod]
        public void REMOVE()
        {
            Assert.IsFalse(true);
        }

        [TestMethod]
        public void HASKEY()
        {
            Assert.IsFalse(true);
        }

        [TestMethod]
        public void KEYS()
        {
            Assert.IsFalse(true);
        }

        [TestMethod]
        public void VALUES()
        {
            Assert.IsFalse(true);
        }
    }
}