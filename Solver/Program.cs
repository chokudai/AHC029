using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

class Field
{
    int N, M, K, T;

    double[] GuessX;

    public Field(int N, int M, int K, int T)
    {
        this.N = N;
        this.M = M;
        this.K = K;
        this.T = T;

    }

}

class State
{
    static public int Field;


}

class Solver
{
    //上げて実行するときは必ずCommitする！
    int SolverVersion = 1;

    public static int Main()
    {


    }


}

