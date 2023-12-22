using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.ExceptionServices;
using System.Security.Cryptography.X509Certificates;

public class Field
{
    public int N, M, K, T;

    //ローカルテスト判定
    public bool LocalFlag;
    //自動テスト判定
    public bool TestFlag;


    public XRand rnd;
    public Stopwatch sw;

    //ローカルテスト用
    public Scanner cin;
    public PreProject [] pps = new PreProject[0];
    public (Card c, int cost)[][] pcs = new (Card c, int cost)[0][];

    public int[] XGuess;
    public int Xsum;

    public Field(int N, int M, int K, int T)
    {
        this.N = N;
        this.M = M;
        this.K = K;
        this.T = T;

        XGuess = new int[] { 21, 11, 11, 6, 3 };
        Xsum = XGuess.Sum();
    }

    public void AddX(int x)
    {
        XGuess[x]++;
        Xsum++;
    }

    public void SetPPS()
    {
        pps = new PreProject[T * M];
        for (int i = 0; i < pps.Length; i++)
        {
            int h = cin.nextInt();
            int v = cin.nextInt();
            pps[i] = new PreProject(v, h);
        }
    }

    public void SetCPS()
    {
        pcs = new (Card c, int cost)[T][];
        for (int i = 0; i < T; i++)
        {
            pcs[i] = new (Card c, int cost)[K];
            for (int j = 0; j < K; j++)
            {
                int t = cin.nextInt();
                int w = cin.nextInt();
                int c = cin.nextInt();
                pcs[i][j] = (new Card(t, w), c);
            }
        }
    }

}

public class PreProject
{
    public int V;
    public int HP;

    public PreProject(int V, int HP)
    {
        this.V = V;
        this.HP = HP;
    }

    public PreProject(XRand rnd)
    {
        double b = rnd.nextDouble() * 6 + 2.0;
        int h = (int)Math.Round(Math.Pow(2, b));

        double p = Math.Min(10.0, Math.Max(0.0, rnd.GetGauss(b, 0.5)));

        int v = (int)Math.Round(Math.Pow(2, p));

        this.V = v;
        this.HP = h;

    }
}


public class Project
{
    public int V;
    public int HP;

    public Project(int V, int HP)
    {
        this.V = V;
        this.HP = HP;
    }

    public Project(PreProject pp, int L)
    {
        this.V = pp.V << L;
        this.HP = pp.HP << L;
    }
}

public class Card
{
    public int type;
    public int work;

    public Card(int type, int work)
    {
        this.type = type;
        this.work = work;
    }
}


public class State
{
    public Field F;

    public Project[] ps;
    public int[] damage;
    public Card[] cs;
    public long money;
    public int Turn;
    public int PreUse;
    public int UsedProject;
    public int L;
    public int PreTarget;

    public State(Field F, Project[] ps, Card[] cs)
    {
        this.F = F;
        Turn = 0;
        money = 0;
        Turn = 0;
        UsedProject = 0;
        PreUse = -1;
        L = 0;
        this.ps = ps;
        this.cs = cs;
        damage = new int[F.M];
        PreTarget = -1;
    }

    public State(State s)
    {
        this.F = s.F;
        this.Turn = s.Turn;
        this.money = s.money;
        this.ps = (Project[])s.ps.Clone();
        this.cs = (Card[])s.cs.Clone();
        this.PreUse = s.PreUse;
        this.UsedProject = s.UsedProject;
        this.L = s.L;
        this.damage = (int[])s.damage.Clone();
        this.PreTarget = s.PreTarget;
    }

    public List<int> UpdateProjects = new List<int>();

    public void Simulate(int Card, int Target)
    {
        UpdateProjects = new List<int>();
        PreUse = Card;

        if (cs[Card].type == 0)
        {
            //通常
            Attack(Target, cs[Card].work);
            PreTarget = Target;

        }
        else if (cs[Card].type == 1)
        {
            for (int i = 0; i < F.M; i++)
            {
                Attack(i, cs[Card].work);
            }
        }
        else if (cs[Card].type == 2)
        {
            Remove(Target);
            if (PreTarget == Card) PreTarget = -1;
        }
        else if (cs[Card].type == 3)
        {
            for (int i = 0; i < F.M; i++)
            {
                Remove(i);
            }
        }
        else
        {
            L++;
        }
        Turn++;
    }

    public void Attack(int target, int d)
    {
        damage[target] += d;

        //Console.Error.WriteLine($"Damage: {damage[target]} Target: {target} HP: {ps[target].HP}");
        if (damage[target] >= ps[target].HP)
        {
            UpdateProjects.Add(target);
            money += ps[target].V;
            damage[target] = 0;
            if(target == PreTarget) PreTarget = -1;
        }
    }

    public void Remove(int target)
    {
        UpdateProjects.Add(target);
        damage[target] = 0;
    }

    public void UpdateFromInput()
    {
        ps = new Project[F.M];
        damage = new int[F.M];
        for (int i = 0; i < F.M; i++)
        {
            int h = F.cin.nextInt();
            int v = F.cin.nextInt();
            ps[i] = new Project(v, h);
        }
        int nextMoney = F.cin.nextInt();
        if(money != nextMoney)
        {
            Console.Error.WriteLine($"Error: money != nextMoney. {money} != {nextMoney}");
            money = nextMoney;
        }
    }

    public void Update()
    {
        if (F.LocalFlag)
        {
            Update(F.pps);
        }
        else
        {
            UpdateFromInput();
        }
    }

    public void Update(PreProject[] pps)
    {
        foreach (var i in UpdateProjects)
        {
            ps[i] = new Project(pps[UsedProject], L);
            UsedProject++;
        }
    }

    public void UpdateFromLPS(List<PreProject> lpp, int up)
    {
        foreach (var i in UpdateProjects)
        {
            int next = UsedProject - up;
            if (lpp.Count <= next)
            {
                lpp.Add(new PreProject(F.rnd));
            }
            ps[i] = new Project(lpp[next], L);
            UsedProject++;
        }
    }


    public Project GetNextProject(int t, int F, PreProject[] pps)
    {
        return new Project(pps[t].V << L, pps[t].HP << L);
    }


    public void BuyCard(Card c, int cost)
    {
        cs[PreUse] = c;
        money -= cost;
        PreUse = -1;
    }
}

public partial class Solver
{


    public static void Main()
    {
        new Solver().run();
    }

    public Solver()
    {
        
    }

    public Field F = new Field(0, 0, 0, 0);
    public State S;
    Scanner cin;

    public Project[] InitProject;
    public Card[] InitCard;

    public void run()
    {
        input();
        init();
        calc();
    }

    void input()
    {
        var OS = Environment.GetEnvironmentVariable("OS");
        cin = new Scanner();
        F.rnd = new XRand();

        if (!F.TestFlag)
        {
            int N = cin.nextInt();
            int M = cin.nextInt();
            int K = cin.nextInt();
            int T = cin.nextInt();

            F = new Field(N, M, K, T);
            F.LocalFlag = (OS != null && OS.Contains("Windows"));
            //F.LocalFlag = false;
            F.cin = cin;
            F.rnd = new XRand();


            InitProject = new Project[M];
            InitCard = new Card[N];
            if (F.LocalFlag)
            {

                for (int i = 0; i < M; i++)
                {
                    int h = cin.nextInt();
                    int v = cin.nextInt();
                    InitProject[i] = new Project(v, h);
                }
                F.SetPPS();

                for (int i = 0; i < N; i++)
                {
                    int t = cin.nextInt();
                    int w = cin.nextInt();
                    InitCard[i] = new Card(t, w);
                }
                F.SetCPS();
            }
            else
            {
                for (int i = 0; i < N; i++)
                {
                    int t = cin.nextInt();
                    int w = cin.nextInt();
                    InitCard[i] = new Card(t, w);
                }

                for (int i = 0; i < M; i++)
                {
                    int h = cin.nextInt();
                    int v = cin.nextInt();
                    InitProject[i] = new Project(v, h);
                }
            }
        }

        S = new State(F, InitProject, InitCard);
    }

    void init()
    {
        F.sw = new Stopwatch();
        F.sw.Start();
    }

    void calc ()
    {
        for (int i = 0; i < F.T; i++)
        {
            if(i != 0)
            {
                var CardList = GetCardList(S);
                for (int j = 1; j < CardList.Length; j++)
                {
                    F.AddX(CardList[j].c.type);
                }

                var choice = choose(S, CardList);
                S.BuyCard(CardList[choice.buy].c, CardList[choice.buy].cost);
                S.Simulate(choice.use.i, choice.use.t);

                //Console.Error.WriteLine($"Buy CardType: {CardList[choice.buy].c.type} Power: {CardList[choice.buy].c.work} Cost:{CardList[choice.buy].cost}");
                //Console.Error.WriteLine($"Use CardType: {S.cs[choice.use.i].type} Power: {S.cs[choice.use.i].work} Target:{choice.use.t}");

                if (!F.TestFlag)
                {
                    Console.WriteLine($"{choice.buy}");
                    Console.WriteLine($"{choice.use.i} {choice.use.t}");
                }
                S.Update();
            }
            else
            {
                var choice = choose(S, null);
                S.Simulate(choice.use.i, choice.use.t);
                //Console.Error.WriteLine($"Use CardType: {S.cs[choice.use.i].type} Power: {S.cs[choice.use.i].work} Target:{choice.use.t}");

                if (!F.TestFlag)
                {
                    Console.WriteLine($"{choice.use.i} {choice.use.t}");
                }
                S.Update();
            }

            if (S.Turn < 3000)
            {
                //Console.Error.WriteLine($"Turn: {S.Turn} Money: {S.money} Level: {S.L} Val: {Eval(S)}");
            }
        }

        if (!F.TestFlag)
        {
            Console.WriteLine("0");
        }

        Console.Error.WriteLine($"Score = {S.money} Level = {S.L}");
    }

    (int buy, (int i, int t) use) choose(State S, (Card c, int cost)[] cs)
    {
        if (cs == null) cs = new (Card c, int cost)[1];
        S.PreTarget = -1;
        List<(long Score, State S, (int buy, (int i, int t) use) play)> ls = new List<(long Score, State S, (int buy, (int i, int t) use) play)>();

        for (int i = 0; i < cs.Length; i++)
        {
            State NS = new State(S);
            if(cs.Length != 1)
            {
                if (S.L == 20 && cs[i].c.type == 4) continue;
                NS.BuyCard(cs[i].c, cs[i].cost);
                if (NS.money < 0) continue;
            }
            for (int j = 0; j < S.cs.Length; j++)
            {
                int K = 1;
                
                if (NS.cs[j].type == 0 || NS.cs[j].type == 2)
                {
                    K = F.M;
                }
                for (int k = 0; k < K; k++)
                {
                    State NS2 = new State(NS);
                    NS2.Simulate(j, k);
                    //NS2.Update();

                    long score = Eval(NS2);

                    ls.Add((score, NS2, (i, (j, k))));
                }
            }
        }

        ls.Sort((a, b) => -a.Score.CompareTo(b.Score));

        
        if (ls.Count == 0) return (0, (0, 0));

        if (F.sw.ElapsedMilliseconds >= 1800) return ls[0].play;

        int Target, CheckNum, CheckTurn;
        if (F.sw.ElapsedMilliseconds >= 1600)
        {
            Target = Math.Min(ls.Count, 2);
            CheckNum = 2;
            CheckTurn = Math.Min(5, F.T - S.Turn - 1);
        }
        else
        {
            double perTime = (1600.0 - F.sw.ElapsedMilliseconds) /(F.T - S.Turn);
            if (perTime >= 2.0)
            {
                Target = Math.Min(ls.Count, 8);
                CheckNum = 20;
                CheckTurn = Math.Min(5, F.T - S.Turn - 1);
            }
            else if (perTime >= 1.6)
            {
                Target = Math.Min(ls.Count, 5);
                CheckNum = 5;
                CheckTurn = Math.Min(5, F.T - S.Turn - 1);
            }
            else if(perTime >= 1.3)
            {
                Target = Math.Min(ls.Count, 3);
                CheckNum = 3;
                CheckTurn = Math.Min(5, F.T - S.Turn - 1);
            }
            else
            {
                Target = Math.Min(ls.Count, 2);
                CheckNum = 2;
                CheckTurn = Math.Min(5, F.T - S.Turn - 1);
            }
        }



        long[] PointSum = new long[Target];




        for (int cn = 0; cn < CheckNum; cn++)
        {
            (Card c, int cost)[][] pcs = MakeCL(F.XGuess, F.Xsum, CheckTurn);
            for (int t = 0; t < CheckTurn; t++)
            {
                pcs[t] = new (Card c, int cost)[F.K];
                pcs[t][0] = (new Card(0, 1), 0);
                for (int i = 1; i < F.K; i++)
                {
                    int T = F.rnd.nextInt(53);

                    int type;
                    if (T < 21) type = 0;
                    else if (T < 32) type = 1;
                    else if (T < 43) type = 2;
                    else if (T < 49) type = 3;
                    else type = 4;

                    int p;
                    int omega = F.rnd.nextInt(1, 50);

                    if (type == 0)
                    {
                        p = (int)Math.Min(10000, Math.Max(1, Math.Round(F.rnd.GetGauss(omega, omega / 3.0))));
                    }
                    else if (type == 1)
                    {
                        p = (int)Math.Min(10000, Math.Max(1, Math.Round(F.rnd.GetGauss(omega, omega / 3.0))));
                    }
                    else if (type == 2 || type == 3)
                    {
                        p = F.rnd.nextInt(0, 10);
                        omega = 0;
                    }
                    else
                    {
                        p = F.rnd.nextInt(200, 1000);
                        omega = 0;
                    }
                    pcs[t][i] = (new Card(type, omega), p);
                }
            }

            List<PreProject> lpp = new List<PreProject>();

            for (int tar = 0; tar < Target; tar++)
            {
                State now = ls[tar].S;
                int start = now.UsedProject;

                for (int t = 0; t < CheckTurn; t++)
                {
                    now.UpdateFromLPS(lpp, start);

                    List<(long Score, State S, (int buy, (int i, int t) use) play)> ls2 = new List<(long Score, State S, (int buy, (int i, int t) use) play)>();

                    var css = GetCardListWithPCS(now, pcs[t]);
                    for (int i = 0; i < css.Length; i++)
                    {
                        State NS = new State(now);
                        if (cs.Length != 1)
                        {
                            if (S.L == 20 && css[i].c.type == 4) continue;
                            NS.BuyCard(css[i].c, css[i].cost);
                            if (NS.money < 0) continue;
                        }
                        for (int j = 0; j < S.cs.Length; j++)
                        {
                            int K = 1;
                            if (NS.cs[j].type == 0 || NS.cs[j].type == 2)
                            {
                                K = F.M;
                            }
                            for (int k = 0; k < K; k++)
                            {
                                if (NS.cs[j].type == 0 && NS.PreTarget != k && NS.PreTarget != -1) continue;

                                State NS2 = new State(NS);
                                NS2.Simulate(j, k);

                                long score = Eval(NS2);

                                ls2.Add((score, NS2, (i, (j, k))));
                            }
                        }
                    }
                    ls2.Sort((a, b) => -a.Score.CompareTo(b.Score));
                    now = ls2[0].S;
                }
                PointSum[tar] += Eval(now);
            }
        }

        int best = 0;
        for (int i = 1; i < Target; i++)
        {
            if (PointSum[best] < PointSum[i])
            {
                best = i;
            }
        }

        return ls[best].play;
    }
    
    long Eval(State S)
    {

        if (F.T <= S.Turn - 1) return S.money;
        if (S.L > 20) return -99999999;

        int NokoriTurn = F.T - S.Turn;

        long AttackAverage = 10 << S.L;
        //if(S.L > 10) AttackAverage += 12 << 10;

        long Need = AttackAverage * 0;
        
        long ans = 0;
        ans += AttackAverage * NokoriTurn / 10;
        ans += (long)S.money * 1;

        if(Need > S.money)
        {
            ans -= (Need - S.money) * 1000;
        }

        //カード評価
        for (int i = 0; i < S.cs.Length; i++)
        {
            if (S.PreUse == i) continue;
            if (S.cs[i].type == 0)
            {
                ans += (long)S.cs[i].work * 89 / 100;

                //if (S.cs[i].work <= (1 << S.L)) ans -= (1 << S.L);
            }
            else if (S.cs[i].type == 1)
            {
                ans += (long)S.cs[i].work * F.M * 4 / 5 * 89 / 100;
            }
            else if (S.cs[i].type == 2 || S.cs[i].type == 3)
            {
                //ans += AttackAverage / 2;
            }
            else if (S.cs[i].type == 4)
            {
                ans += 0;
            }

        }

        //プロジェクト評価
        for (int i = 0; i < F.M; i++)
        {
            ans += (S.ps[i].V - (S.ps[i].HP - S.damage[i]) * 93 / 100);

            double needTurn = (double)(S.ps[i].HP - S.damage[i]) / AttackAverage + 1;
            ans += (long)(AttackAverage / needTurn / 2);

            //double needTurn = Math.Max(1, (double)(S.ps[i].HP - S.damage[i]) / AttackAverage);
            //ans += S.ps[i].V - (long)(needTurn * AttackAverage);
        }
        foreach (var i in S.UpdateProjects)
        {
            ans -= (S.ps[i].V - (S.ps[i].HP - S.damage[i]) * 93 / 100);

            double needTurn = (double)(S.ps[i].HP - S.damage[i]) / AttackAverage + 1;
            ans -= (long)(AttackAverage / needTurn / 2);

            ans += (long)AttackAverage;
            //double needTurn = Math.Max(1, (double)(S.ps[i].HP - S.damage[i]) / AttackAverage);
            //ans -= S.ps[i].V - (long)(needTurn * AttackAverage);
        }

        return ans;
    }


    public (Card c, int cost)[][] MakeCL(int[] GuessX, int Sum, int CheckTurn)
    {
        (Card c, int cost)[][] pcs = new (Card c, int cost)[CheckTurn][];
        for (int t = 0; t < CheckTurn; t++)
        {
            pcs[t] = new (Card c, int cost)[F.K];
            pcs[t][0] = (new Card(0, 1), 0);
            for (int i = 1; i < F.K; i++)
            {
                int T = F.rnd.nextInt(Sum);

                int type;
                if ((T -= GuessX[0]) < 0) type = 0;
                else if ((T -= GuessX[1]) < 0) type = 1;
                else if ((T -= GuessX[2]) < 0) type = 2;
                else if ((T -= GuessX[3]) < 0) type = 3;
                else type = 4;

                int p;
                int omega = F.rnd.nextInt(1, 50);

                if (type == 0)
                {
                    p = (int)Math.Min(10000, Math.Max(1, Math.Round(F.rnd.GetGauss(omega, omega / 3.0))));
                }
                else if (type == 1)
                {
                    p = (int)Math.Min(10000, Math.Max(1, Math.Round(F.rnd.GetGauss(omega, omega / 3.0))));
                }
                else if (type == 2 || type == 3)
                {
                    p = F.rnd.nextInt(0, 10);
                    omega = 0;
                }
                else
                {
                    p = F.rnd.nextInt(200, 1000);
                    omega = 0;
                }
                pcs[t][i] = (new Card(type, omega), p);
            }
        }
        return pcs;
    }


    (Card c, int cost)[] GetCardList(State S)
    {
        if(F.LocalFlag)
        {
            return GetCardListWithPCS(S, F.pcs[S.Turn - 1]);
        }
        else
        {
            (Card c, int cost)[] ret = new (Card c, int cost)[F.K];
            for (int i = 0; i < F.K; i++)
            {
                int t = cin.nextInt();
                int w = cin.nextInt();
                int c = cin.nextInt();
                ret[i] = (new Card(t, w), c);
            }
            return ret;
        }
    }

    (Card c, int cost)[] GetCardListWithPCS(State S, (Card c, int cost)[] pcs)
    {
        (Card c, int cost)[] ret = new (Card c, int cost)[F.K];
        for (int i = 0; i < F.K; i++)
        {
            ret[i] = (new Card(pcs[i].c.type, pcs[i].c.work << S.L), pcs[i].cost << S.L);
        }
        return ret;
    }




}

public class Scanner
{
    string[] s;
    int i;

    char[] cs = new char[] { ' ' };

    public Scanner()
    {
        s = new string[0];
        i = 0;
    }

    public string next()
    {
        if (i < s.Length) return s[i++];
        string st = Console.ReadLine();
        while (st == "") st = Console.ReadLine();
        s = st.Split(cs, StringSplitOptions.RemoveEmptyEntries);
        if (s.Length == 0) return next();
        i = 0;
        return s[i++];
    }

    public int nextInt()
    {
        return int.Parse(next());
    }
    public int[] ArrayInt(int N, int add = 0)
    {
        int[] Array = new int[N];
        for (int i = 0; i < N; i++)
        {
            Array[i] = nextInt() + add;
        }
        return Array;
    }

    public long nextLong()
    {
        return long.Parse(next());
    }

    public long[] ArrayLong(int N, long add = 0)
    {
        long[] Array = new long[N];
        for (int i = 0; i < N; i++)
        {
            Array[i] = nextLong() + add;
        }
        return Array;
    }

    public double nextDouble()
    {
        return double.Parse(next());
    }


    //swap
    void swap<T>(ref T a, ref T b)
    {
        T c = a;
        a = b;
        b = c;
    }
    public double[] ArrayDouble(int N, double add = 0)
    {
        double[] Array = new double[N];
        for (int i = 0; i < N; i++)
        {
            Array[i] = nextDouble() + add;
        }
        return Array;
    }
}



public class XRand
{
    uint x, y, z, w;


    public XRand()
    {
        init();
    }

    public XRand(uint s)
    {
        init();
        init_xor128(s);
    }

    void init()
    {
        x = 314159265; y = 358979323; z = 846264338; w = 327950288;

    }

    public void init_xor128(uint s)
    {
        z ^= s;
        z ^= z >> 21; z ^= z << 35; z ^= z >> 4;
        z *= 736338717;
    }

    uint next()
    {
        uint t = x ^ x << 11; x = y; y = z; z = w; return w = w ^ w >> 19 ^ t ^ t >> 8;
    }

    public long nextLong(long m)
    {
        return (long)((((ulong)next() << 32) + next()) % (ulong)m);
    }

    public int nextInt(int m)
    {
        return (int)(((long)next() * m) >> 32);
    }

    public int nextIntP(int a)
    {
        return (int)Math.Pow(a, nextDouble());
    }

    public int nextInt(int min, int max)
    {
        return min + nextInt(max - min + 1);
    }

    public double GetGauss(double ave, double norm)
    {
        double ret = ave + normRand() * norm;
        return ret;
    }

    public double normRand()
    {
        return Math.Sqrt(-2 * Math.Log(nextDouble())) * Math.Cos(2 * 3.14159265358979 * nextDouble());
    }

    public double nextDouble()
    {
        return (double)next() / uint.MaxValue;
    }

    public double nextDoubleP(double a)
    {
        return Math.Pow(a, nextDouble());
    }
}