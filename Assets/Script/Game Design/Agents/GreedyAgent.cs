﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Assets.Script.Game_Design;
using Assets.Script.Game_Design.Enums;

public class GreedyAgent : BaseAgent
{
    
    /*
    *  The greedy agent checks all of its possible moves to see what the outcome is.
    *  If any result in a win, it takes them. If any result in a loss, it never takes them.
    *  Otherwise it randomly plays. It's sort of like RandomAgent++.
    *
    *  In games with score, or some way of measuring intermediate reward, greedy agents can 
    *  pick the action with the best short-term reward. Sometimes you can measure this
    *  (for example, we could set it so that if the aim is to get 5 in a row, it tries to
    *  make a move that will get it 4-in-a-row, if it can't do that then 3-in-a-row, etc).
    *  
    *  However, this is the kind of heuristic that is often very hard to define, which is one
    *  of the really interesting challenges in designing AGD systems! 
    *  
    */

    public GreedyAgent(int playerCode){
        this.playerCode = playerCode;
    }

    /// <summary>
    /// Take a turn, but use no more than the allotted time limit.
    /// </summary>
    /// <param name="g">The game to play</param>
    /// <param name="timeLimit">Time limit in seconds</param>
    /// <returns>Returns whether we managed to take a turn or not.</returns>
    public override bool TakeTurn(Game g, float timeLimit = 1f){
        List<Point> bestActions = new List<Point>();
        int bestResult = 0;

        bool wasInteractive = g.interactiveMode;
        g.interactiveMode = false;
        
        //NOTE: use timeLimit in addition to cutoff, if TapAction takes a lot of time for some reason.
        var timer = Stopwatch.StartNew();
        var timeLimitInMillis = timeLimit * 1000f;

        //Use local function, to be able to break out of nested loop.
        void EvaluatePossibleActions()
        {
            for(int i=0; i<g.boardWidth; i++){
                for(int j=0; j<g.boardHeight; j++){
                
                    //Check if the time is up! Break out of nested loop using return in local function.
                    if (timer.ElapsedMilliseconds > timeLimitInMillis) return;
                
                    g.SaveGameState();
                    if(g.TapAction(i, j)){
                        //If the outcome is as good as the best outcome we've found, add it to the list
                        if(g.endStatus == bestResult){
                            bestActions.Add(new Point(i, j));
                        }
                        //If the best we've found so far is nothing, but this result is us winning, great.
                        if(g.endStatus == playerCode && bestResult == 0){
                            bestResult = playerCode;
                            bestActions = new List<Point>();
                            bestActions.Add(new Point(i, j));
                        }
                        // Note that we don't consider draws to be better than nothing. This is quite a
                        // nuanced question and depends on the game and the situation. You could write
                        // a smarter AI that contemplated its odds, but we're just going to worry about
                        // winning and losing here (plus thinking ahead isn't the Greedy style ^_^)

                        g.RestoreSavedState();
                    } 
                }
            }
        }
        
        //Run local function.
        EvaluatePossibleActions();

        g.interactiveMode = wasInteractive;

        if(bestActions.Count == 0) {
            //If there's nothing we can do, pick a random action
            for(int i=0; i<1000; i++){
                
                if(g.TapAction(Random.Range(0, g.boardWidth), Random.Range(0, g.boardHeight))){
                    return true;
                }
                
                //Check if the time is up, after the first try,
                //this allows it to choose one random action after breaking out of the evaluate function!
                if (timer.ElapsedMilliseconds > timeLimitInMillis) break;
            }
            // Debug.LogError("Unable to take a move");
            return false;
        }
        else{
            //Otherwise pick whatever action was best
            Point ba = bestActions[Random.Range(0, bestActions.Count)];
            g.TapAction(ba.x, ba.y);
        }
        return true;

    }

}
