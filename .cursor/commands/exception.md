The game has encountered a C# exception in production. Often this is caused by an Entity or Component not being Alive (our equivalent of null) when it is dereferenced.

Carefully review the stack trace and analyze the potential flow checking for conditions where players could leave or entities getting destroyed in a way we didn't anticipate and check if (e.Alive(). You **DO NOT** need to check for null since Alive checks implicitly do so. 

If the issue doesn't appear to be related to component Alive'ness, carefully review and identify the actual root cause. 

Only relevant to Effects, note that fields in effects that are set dynamically (for example in a preInit function) will be null for clients joining the game mid-effect unless they are explicitly serialized. If you suspect this is the case implement the INetworkedComponent interface in the effect to serialize/deserialize the field value. 

If the exception occurred inside NetworkHandler.g.cs, the actual offending code will be inside a [ClientRpc] or [ServerRpc]. To get a hint for which one, you can manually open /generated/NetworkHandler.g.cs and look at the line number BUT the line number may be useless if the developer's copy of the game differs from what's in production. If you suspect this is the case (the code at the offending line number doesn't seem to correlate to this issue) carefully review all RPCs in the main game code and see if you find anything that matches the behavior of the exception reported.

After you make your adjustments you MUST carefully analyze the potential downstream effects of your change and explain these to the user. 

Here is the stack trace to analyze: