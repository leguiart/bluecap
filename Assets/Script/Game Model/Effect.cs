using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets.Script.Game_Design;

public class Effect
{
    public virtual void Apply(Game g){
        
    }

    public virtual string Print(){
        return "Generic effect";
    }

    public virtual string ToCode(){
        return "<error - did not override ToCode()>";
    }

}
