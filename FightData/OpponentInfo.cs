using Divine;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Collections;

public class OpponentInfo
{
    public string       name        { get; private set; }
    public int          trophy      { get; private set; }
    public PartyInfo    partyInfo   { get; private set; }

    public OpponentInfo(PartyData data)
    {
        name    = data.Name;
        trophy  = data.Trophy;
    }
}
