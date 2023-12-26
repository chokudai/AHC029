using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.XPath;

class Tester
{
    int TargetNum = 5;
    int N_Min = 2;
    int N_Max = 7;
    int M_Min = 2;
    int M_Max = 8;
    int K_Min = 2;
    int K_Max = 5;

    public static void Main()
    {
        new Tester().run();
    }

    void run()
    {
        string FolderName = string.Join("-", new int[] { N_Min, N_Max, M_Min, M_Max, K_Min, K_Max, TargetNum });

        //FolderName = "../../../" + FolderName;
        FolderName = "./" + FolderName;
        if (!Directory.Exists(FolderName))
        {
            Directory.CreateDirectory(FolderName);
        }

        int KMUL = K_Max - K_Min + 1;
        int MMUL = M_Max - M_Min + 1;
        int NMUL = N_Max - N_Min + 1;

        int All = KMUL * MMUL * NMUL * TargetNum;
        (int n, int m, int k, long score)[] result = new (int n, int m, int k, long score)[All];

        Console.Error.WriteLine($"CheckPattern: {All}");
        string FileName = $"{FolderName}/{Solver.SolverVersion:0000}.txt";
        if (File.Exists(FileName))
        {
            Console.Error.WriteLine("注意：もう実行されているバージョンです。このまま実行すると上書きします。");
            Console.ReadLine();
        }

        int CaseNumber = 0;
        //for (int tn = 0; tn < TargetNum; tn++)
        //{
        //    for (int n = N_Min; n <= N_Max; n++)
        //    {
        Parallel.For(0, TargetNum, tn =>
        {
            //Parallel.For(N_Min, N_Max + 1, n =>
            //{
            for (int n = N_Min; n <= N_Max; n++)
                {
                for (int m = M_Min; m <= M_Max; m++)
                {
                    for (int k = K_Min; k <= K_Max; k++)
                    {
                        int CaseNum = ((tn * NMUL + (n - N_Min)) * MMUL + (m - M_Min)) * KMUL + (k - K_Min); 
                        XRand rnd = new XRand();
                        rnd.init_xor128((uint)CaseNum);

                        Field F = new Field(n, m, k, 1000);
                        F.rnd = rnd;
                        F.TestFlag = true;
                        F.LocalFlag = true;

                        int[] X = new int[5];
                        X[0] = rnd.nextInt(1, 20);
                        X[1] = rnd.nextInt(1, 10);
                        X[2] = rnd.nextInt(1, 10);
                        X[3] = rnd.nextInt(1, 5);
                        X[4] = rnd.nextInt(1, 3);

                        Solver sol = new Solver();
                        sol.F = F;
                        F.pcs = sol.MakeCL(X, X.Sum(), 1000);
                        F.pps = new PreProject[1000 * m];
                        for (int ii = 0; ii < F.pps.Length; ii++)
                        {
                            F.pps[ii] = new PreProject(rnd);
                        }

                        sol.InitProject = new Project[m];
                        for (int ii = 0; ii < m; ii++)
                        {
                            sol.InitProject[ii] = new Project(new PreProject(rnd), 0);
                        }
                        sol.InitCard = new Card[n];
                        var fpcs = sol.MakeCL(X, X.Sum(), 5);
                        for (int ii = 0; ii < n; ii++)
                        {
                            sol.InitCard[ii] = fpcs[ii / k][ii % k].c;
                        }

                        sol.run();

                        result[CaseNum] = (n, m, k, sol.ALLS.money);

                        Console.WriteLine($"{tn} {n} {m} {k} : {sol.ALLS.money}");
                    }
                }
            }//);
        });

        List<long> ll = new List<long>();
        foreach (var item in result)
        {
            ll.Add(item.score);
        }

        WriteNumbersToFile(ll.ToArray(), FileName);

        var list = ReadNumbersFromFilesInFolder(FolderName);
        list.Sort();
        list.Reverse();

        long[] MaxScore = new long[All];
        foreach (var item in list)
        {
            for (int i = 0; i < item.Numbers.Length; i++)
            {
                MaxScore[i] = Math.Max(MaxScore[i], item.Numbers[i]);
            }
        }



        List<long> ScoreBorder = new List<long>();
        long[] ScoreArray = (long[])MaxScore.Clone();
        Array.Sort(ScoreArray);
        for (int i = 0; i < 10; i++)
        {
            ScoreBorder.Add(ScoreArray[ScoreArray.Length * i / 10]);
        }
        ScoreBorder.Add(ScoreArray[ScoreArray.Length - 1] + 1);


        List<string> Title = new List<string>();
        List<string> Version = new List<string>();


        Title.Add("Version");
        Title.Add("All");
        for (int n = N_Min; n <= N_Max; n++)
        {
            Title.Add($"N = {n}");
        }
        for (int m = M_Min; m <= M_Max; m++)
        {
            Title.Add($"M = {m}");
        }
        for (int k = K_Min; k <= K_Max; k++)
        {
            Title.Add($"K = {k}");
        }

        for (int i = 0; i < ScoreBorder.Count - 1; i++)
        {
            Title.Add($"{ScoreBorder[i]} - {ScoreBorder[i + 1] - 1}");
        }

        double[][] data = new double[list.Count][];

        List<(long BestScore, (int n, int m, int k, long score) result)> resultList = new List<(long BestScore, (int n, int m, int k, long score) result)>();

        for (int i = 0; i < list.Count; i++)
        {
            var item = list[i];
            Version.Add(item.FileName);
            data[i] = new double[Title.Count - 1];
            int[] countNum = new int[Title.Count - 1];


            long sumScore = 0;

            for (int j = 0; j < All; j++)
            {
                double score = (double)item.Numbers[j] / MaxScore[j];
                int nowP = 0;
                data[i][nowP] += score;
                countNum[nowP]++;
                nowP++;
                sumScore += item.Numbers[j];

                if(i == 0)
                {
                    resultList.Add((MaxScore[j], result[j]));
                }

                for (int n = 0; n < NMUL; n++)
                {
                    if (result[j].n == n + N_Min)
                    {
                        data[i][nowP] += score;
                        countNum[nowP]++;
                    }
                    nowP++;
                }

                for (int m = 0; m < MMUL; m++)
                {
                    if (result[j].m == m + M_Min)
                    {
                        data[i][nowP] += score;
                        countNum[nowP]++;
                    }
                    nowP++;
                }

                for (int k = 0; k < KMUL; k++)
                {
                    if (result[j].k == k + K_Min)
                    {
                        data[i][nowP] += score;
                        countNum[nowP]++;
                    }
                    nowP++;
                }

                for (int k = 0; k < ScoreBorder.Count - 1; k++)
                {
                    if (MaxScore[j] >= ScoreBorder[k] && MaxScore[j] < ScoreBorder[k + 1])
                    {
                        data[i][nowP] += score;
                        countNum[nowP]++;
                    }
                    nowP++;
                }
            }

            for (int j = 0; j < Title.Count - 1; j++)
            {
                if (countNum[j] != 0)
                {
                    data[i][j] /= countNum[j];
                }
                data[i][j] *= 100;
            }

            //Console.Error.WriteLine(item.FileName + " " + sumScore);
        }


        resultList.Sort((a, b) => a.BestScore.CompareTo(b.BestScore));

        foreach (var i in resultList)
        {
            double RScore = (double)i.result.score / i.BestScore;
            Console.WriteLine($"N:{i.result.n} M={i.result.m} K={i.result.k} RScore:{RScore:0.000} Score:{i.BestScore} -> {i.result.score}");
        }

        string htmlFile = $"{FolderName}/summary.html";

        string html = GenerateHtmlTable(Title.ToArray(), Version.ToArray(), data);

        WriteFile(htmlFile, html);
    }


    public static string GenerateHtmlTable(string[] headers, string[] rowHeaders, double[][] values)
    {
        StringBuilder html = new StringBuilder();

        // HTML Tableの基本構造を作成
        html.Append("<table border='1'>");

        // 横の見出しを追加
        html.Append("<tr>");
        foreach (var header in headers)
        {
            html.AppendFormat("<th>{0}</th>", header);
        }
        html.Append("</tr>");

        // 各行とセルの値を追加
        for (int i = 0; i < values.Length; i++)
        {
            html.Append("<tr>");

            // 縦の見出しを追加
            html.AppendFormat("<th>{0}</th>", rowHeaders[i]);

            // セルの値を追加
            for (int j = 0; j < values[i].Length; j++)
            {
                html.AppendFormat("<td style='background-color: {0}'>{1:0.000000}</td>", GetColor(values[i][j]), values[i][j]);
            }

            html.Append("</tr>");
        }

        html.Append("</table>");

        return html.ToString();
    }

    private static string GetColor(double value)
    {
        // 色の計算（赤から白へのグラデーション）
        int red = (int)(255 * (1 - value / 100.0));
        int green = (int)(255 * value / 100.0);
        int blue = (int)(255 * value / 100.0);

        return $"rgb({red}, {green}, {blue})";
    }


    static List<FileNumberPair> ReadNumbersFromFilesInFolder(string folderPath)
    {
        var pairs = new List<FileNumberPair>();

        foreach (var filePath in Directory.GetFiles(folderPath, "*.txt"))
        {
            try
            {
                var content = File.ReadAllText(filePath);
                var numbers = content.Split(',')
                                     .Select(long.Parse)
                                     .ToArray();

                pairs.Add(new FileNumberPair
                {
                    FileName = Path.GetFileName(filePath),
                    Numbers = numbers
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラーが発生しました: {ex.Message}");
            }
        }
        return pairs;
    }


    class FileNumberPair:IComparable<FileNumberPair>
    {
        public string FileName { get; set; }
        public long[] Numbers { get; set; }

        public int CompareTo(FileNumberPair? other)
        {
            return FileName.CompareTo(other.FileName);
        }
    }

    static void WriteNumbersToFile(long[] numbers, string fileName)
    {
        string content = string.Join(",", numbers);
        WriteFile(fileName, content);
    }

    static void WriteFile(string fileName, string content)
    {
        try
        {
            // ファイルに書き込み
            File.WriteAllText(fileName, content);
            Console.WriteLine("ファイルに書き込みました。");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"エラーが発生しました: {ex.Message}");
        }
    }



}