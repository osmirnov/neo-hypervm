#include "ExecutionScript.h"
#include "Crypto.h"
#include <string.h>

ExecutionScript::ExecutionScript(byte* script, int32 scriptLength) :
	IClaimable(),
	_isScriptHashCalculated(false),
	ScriptLength(scriptLength)
{
	// Copy script

	this->Content = new byte[scriptLength];
	memcpy(this->Content, script, scriptLength);
}

int32 ExecutionScript::GetScriptHash(byte* hash)
{
	if (!this->_isScriptHashCalculated)
	{
		// Compute script hash

		this->_isScriptHashCalculated = true;
		Crypto::ComputeHash160(this->Content, this->ScriptLength, &this->ScriptHash[0]);
	}

	memcpy(hash, this->ScriptHash, this->ScriptHashLength);
	return this->ScriptHashLength;
}

bool ExecutionScript::IsTheSameHash(byte* hash, int32 length)
{
	if (length != this->ScriptHashLength) return false;

	if (!this->_isScriptHashCalculated)
	{
		// Compute script hash

		this->_isScriptHashCalculated = true;
		Crypto::ComputeHash160(this->Content, this->ScriptLength, &this->ScriptHash[0]);
	}

	for (int32 x = 0; x < this->ScriptHashLength; x++)
		if (this->ScriptHash[x] != hash[x])
			return false;

	return true;
}

void ExecutionScript::Free(ExecutionScript* &item)
{
	if (item != nullptr && item->IsUnClaimed())
	{
		delete(item);
		item = nullptr;
	}
}

void ExecutionScript::UnclaimAndFree(ExecutionScript* &item)
{
	if (item != nullptr && item->UnClaim())
	{
		delete(item);
		item = nullptr;
	}
}