#pragma once

#include "EVMOpCode.h"
#include "Types.h"

class ExecutionContext
{
private:

	int32 Claims;

public:

	// Constants

	static const int32 ScriptHashLength = 20;

	const bool IsPushOnly;
	const int32 ScriptLength;

	// Position

	int32 InstructionPointer;

	// Reads

	int32 Read(byte * data, int32 length);

	bool ReadUInt8(byte &ret);
	bool ReadUInt16(uint16 &ret);
	bool ReadInt16(int16 &ret);
	bool ReadUInt32(uint32 &ret);
	bool ReadInt32(int32 &ret);
	bool ReadUInt64(uint64 &ret);
	bool ReadInt64(int64 &ret);
	bool ReadVarBytes(uint32 &ret, uint32 max);

	// Get/Read next instruction

	EVMOpCode GetNextInstruction();
	EVMOpCode ReadNextInstruction();
	void Seek(int32 position);

	// Get script hash

	int32 GetScriptHash(byte* hash);

	// Clone execution context

	ExecutionContext* Clone();

	// Constructor

	ExecutionContext(byte* script, int32 scriptLength, bool pushOnly, int32 instructorPointer);

	// Destructor

	~ExecutionContext();
	
	// Claims

	void Claim();
	void UnClaim();

	static void Free(ExecutionContext* &item);
	static void UnclaimAndFree(ExecutionContext* &item);

private:

	// Script

	byte* Script;
	bool IsScriptHashCalculated;
	byte ScriptHash[ScriptHashLength];
};