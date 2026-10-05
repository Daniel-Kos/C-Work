using System.Security.Cryptography;
using System.Text;

class Program
{
    const string LOW  = "abcdefghijklmnopqrstuvwxyz";
    const string UPP  = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string DIG  = "0123456789";
    const string SYM  = "!@#$%^&*()-_=+[]{};:,.<>?/";
    
    static string[] popular =
    {
        "123456","password","123456789","12345678","12345","1234567",
        "qwerty","abc123","111111","123123","admin","letmein",
        "welcome","monkey","dragon","master","sunshine","princess",
        "iloveyou","football","baseball","shadow","superman","qazwsx",
        "password1","1234","000000","qwerty123"
    };
    
    static string[] words = (
        "кот пёс дом лес река море гора небо солнце луна " +
        "звезда ветер дождь снег огонь вода земля камень дерево цветок " +
        "птица рыба волк лиса заяц медведь тигр лев слон жираф " +
        "зебра обезьяна панда кит дельфин акула орёл сокол сова ворон " +
        "голубь воробей синица снегирь ёж барсук бобр выдра крот мышь " +
        "крыса хомяк кролик овца коза корова лошадь свинья курица петух " +
        "утка гусь лебедь аист цапля журавль павлин попугай пингвин страус " +
        "яблоко груша слива вишня персик абрикос виноград банан апельсин лимон " +
        "мандарин арбуз дыня клубника малина смородина крыжовник черника голубика ежевика " +
        "картофель морковь свекла лук чеснок капуста огурец помидор перец баклажан " +
        "кабачок тыква редис репа горох фасоль кукуруза пшеница рожь овёс " +
        "хлеб молоко сыр масло мясо яйцо соль сахар мёд пирог"
    ).Split(' ', StringSplitOptions.RemoveEmptyEntries);

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("=== Анализ и генерация паролей ===");
            Console.WriteLine("1. Проверить пароль");
            Console.WriteLine("2. Сгенерировать пароль");
            Console.WriteLine("3. Сгенерировать парольную фразу");
            Console.WriteLine("4. Сравнить время перебора (быстрый / медленный хеш)");
            Console.WriteLine("0. Выход");
            Console.Write("Выбор: ");

            string choice = (Console.ReadLine() ?? "").Trim();
            Console.WriteLine();

            if (choice == "1") CheckPassword();
            else if (choice == "2") GenerateMenu();
            else if (choice == "3") PassphraseMenu();
            else if (choice == "4") CompareSpeeds();
            else if (choice == "0") return;
            else Console.WriteLine("Неверный ввод.");
        }
    }

    // ---------------- Шаг 1. Размер алфавита ----------------
    static int AlphabetSize(string p)
    {
        bool lo = false, up = false, dg = false, other = false, nonAscii = false;
        foreach (char c in p)
        {
            if (c >= 'a' && c <= 'z') lo = true;
            else if (c >= 'A' && c <= 'Z') up = true;
            else if (c >= '0' && c <= '9') dg = true;
            else if (c < 128) other = true;
            else nonAscii = true;
        }
        int size = 0;
        if (lo) size += 26;
        if (up) size += 26;
        if (dg) size += 10;
        if (other) size += 33;
        if (nonAscii) size += 66;
        return size;
    }

    // ---------------- Шаг 2. Стойкость и оценка ----------------
    static double StrengthBits(string p)
    {
        if (p.Length == 0) return 0;
        int a = AlphabetSize(p);
        if (a <= 1) return 0;
        return p.Length * Math.Log2(a);
    }

    static string Rating(double bits)
    {
        if (bits < 28) return "очень слабый";
        if (bits < 36) return "слабый";
        if (bits < 60) return "средний";
        if (bits < 128) return "сильный";
        return "очень сильный";
    }

    // ---------------- Шаг 3. Время перебора ----------------
    static double BruteSeconds(string p, double speed)
    {
        if (p.Length == 0) return 0;
        int a = AlphabetSize(p);
        double combos = Math.Pow(a, p.Length);
        return combos / 2.0 / speed;
    }

    static string FormatTime(double s)
    {
        double yearSec = 365.25 * 24 * 3600;
        double universe = 13.8e9;
        if (s <= 0) return "мгновенно";

        double years = s / yearSec;
        if (years > universe) return "больше возраста Вселенной";

        if (s < 1) return s.ToString("F4") + " сек";
        if (s < 60) return s.ToString("F1") + " сек";
        if (s < 3600) return (s / 60).ToString("F1") + " мин";
        if (s < 86400) return (s / 3600).ToString("F1") + " ч";
        if (s < yearSec)
        {
            double d = s / 86400;
            int di = Math.Max(1, (int)Math.Round(d));
            return d.ToString("F1") + " " + Plural(di, "день", "дня", "дней");
        }
        if (years < 1e3)
        {
            int yi = Math.Max(1, (int)Math.Round(years));
            return years.ToString("F1") + " " + Plural(yi, "год", "года", "лет");
        }
        if (years < 1e6) return (years / 1e3).ToString("F1") + " тыс. лет";
        if (years < 1e9) return (years / 1e6).ToString("F1") + " млн лет";
        return (years / 1e9).ToString("F1") + " млрд лет";
    }

    static string Plural(int n, string one, string few, string many)
    {
        int m10 = Math.Abs(n) % 10, m100 = Math.Abs(n) % 100;
        if (m10 == 1 && m100 != 11) return one;
        if (m10 >= 2 && m10 <= 4 && (m100 < 10 || m100 >= 20)) return few;
        return many;
    }

    // ---------------- Шаг 4. Словарь и шаблоны ----------------
    static bool InDictionary(string p, out string found)
    {
        found = null;
        string low = p.ToLowerInvariant();
        foreach (var w in popular)
            if (w == low) { found = w; return true; }
        return false;
    }

    // Ловим: (1) один повторяющийся символ, (2) слово + цифры в конце,
    // (3) последовательности вида 12345 или abcde.
    static bool PatternBased(string p, out string reason)
    {
        reason = null;
        if (p.Length < 2) return false;

        if (p.Distinct().Count() == 1)
        {
            reason = "состоит из одного повторяющегося символа";
            return true;
        }

        string low = p.ToLowerInvariant();
        string[] bases =
        {
            "password","qwerty","admin","letmein","welcome",
            "monkey","dragon","master","shadow","superman","iloveyou"
        };
        foreach (var w in bases)
        {
            if (low.StartsWith(w))
            {
                string tail = p.Substring(w.Length);
                if (tail.Length > 0 && tail.All(char.IsDigit))
                {
                    reason = "популярное слово \"" + w + "\" с цифрами в конце";
                    return true;
                }
            }
        }

        if (HasSequence(p))
        {
            reason = "есть последовательность (12345, abcde и т.п.)";
            return true;
        }
        return false;
    }

    static bool HasSequence(string s)
    {
        if (s.Length < 5) return false;
        string low = s.ToLowerInvariant();
        for (int i = 0; i + 5 <= low.Length; i++)
        {
            bool asc = true, desc = true;
            for (int j = 1; j < 5; j++)
            {
                if (low[i + j] != low[i + j - 1] + 1) asc = false;
                if (low[i + j] != low[i + j - 1] - 1) desc = false;
            }
            if (asc || desc) return true;
        }
        return false;
    }

    // ---------------- Шаг 5. Генератор пароля ----------------
    static string GeneratePassword(int len, bool lo, bool up, bool dg, bool sy)
    {
        if (len < 8 || len > 64) throw new Exception("Длина должна быть от 8 до 64.");
        var groups = new List<string>();
        if (lo) groups.Add(LOW);
        if (up) groups.Add(UPP);
        if (dg) groups.Add(DIG);
        if (sy) groups.Add(SYM);
        if (groups.Count == 0) throw new Exception("Выберите хотя бы одну группу.");
        if (len < groups.Count) throw new Exception("Длина меньше числа групп.");

        var chars = new List<char>();
        // Гарантируем по одному символу из каждой группы.
        foreach (var g in groups)
            chars.Add(g[RandomNumberGenerator.GetInt32(g.Length)]);

        // Добираем остальное из общего пула.
        string pool = string.Concat(groups);
        while (chars.Count < len)
            chars.Add(pool[RandomNumberGenerator.GetInt32(pool.Length)]);

        Shuffle(chars);
        return new string(chars.ToArray());
    }

    // Алгоритм Фишера - Йетса.
    static void Shuffle(List<char> a)
    {
        for (int i = a.Count - 1; i > 0; i--)
        {
            int j = RandomNumberGenerator.GetInt32(i + 1);
            (a[i], a[j]) = (a[j], a[i]);
        }
    }

    // ---------------- Шаг 6. Парольная фраза ----------------
    static string GeneratePassphrase(int count)
    {
        if (count < 2) throw new Exception("Нужно минимум 2 слова.");
        var parts = new List<string>();
        for (int i = 0; i < count; i++)
            parts.Add(words[RandomNumberGenerator.GetInt32(words.Length)]);
        return string.Join("-", parts);
    }

    static double PassphraseBits(int count) =>
        count * Math.Log2(words.Length);

    // ---------------- Меню: проверка пароля ----------------
    static void CheckPassword()
    {
        Console.Write("Введите пароль: ");
        string p = ReadMasked();
        Console.WriteLine();

        if (p.Length == 0) { Console.WriteLine("Пароль пустой."); return; }

        Console.WriteLine("Длина:           " + p.Length);
        Console.WriteLine("Размер алфавита: " + AlphabetSize(p));

        if (InDictionary(p, out string found))
        {
            Console.WriteLine("Найден в словаре популярных паролей (\"" + found + "\").");
            Console.WriteLine("Подбирается мгновенно.");
            return;
        }

        double bits = StrengthBits(p);
        string rating = Rating(bits);

        if (PatternBased(p, out string reason))
        {
            Console.WriteLine("Обнаружен шаблон: " + reason + ".");
            Console.WriteLine("Оценка понижена.");
            rating = "слабый";
            if (bits > 36) bits = 36;
        }

        Console.Write("Стойкость: " + bits.ToString("F1") + " бит, оценка: ");
        Colored(rating);
        Console.WriteLine();

        Console.WriteLine("Перебор при 10 млрд/с:              " +
                          FormatTime(BruteSeconds(p, 1e10)));
        Console.WriteLine("Перебор при медленном хеше (10 тыс/с): " +
                          FormatTime(BruteSeconds(p, 1e4)));
    }

    // ---------------- Меню: генерация пароля ----------------
    static void GenerateMenu()
    {
        Console.Write("Длина (8-64): ");
        if (!int.TryParse(Console.ReadLine(), out int len))
        { Console.WriteLine("Не число."); return; }

        bool lo = Ask("Строчные? (y/n): ", true);
        bool up = Ask("Заглавные? (y/n): ", true);
        bool dg = Ask("Цифры? (y/n): ", true);
        bool sy = Ask("Символы? (y/n): ", false);

        try
        {
            string p = GeneratePassword(len, lo, up, dg, sy);
            Console.WriteLine();
            Console.WriteLine("Пароль: " + p);
            double bits = StrengthBits(p);
            Console.Write("Стойкость: " + bits.ToString("F1") + " бит, оценка: ");
            Colored(Rating(bits));
            Console.WriteLine();
            Console.WriteLine("Перебор: " + FormatTime(BruteSeconds(p, 1e10)));

            Console.WriteLine();
            Console.WriteLine("Проверка 10 паролей на все группы:");
            for (int i = 1; i <= 10; i++)
            {
                string q = GeneratePassword(len, lo, up, dg, sy);
                bool ok =
                    (!lo || HasAny(q, LOW)) &&
                    (!up || HasAny(q, UPP)) &&
                    (!dg || HasAny(q, DIG)) &&
                    (!sy || HasAny(q, SYM));
                Console.WriteLine("  " + i + ". " + q + "  [" + (ok ? "OK" : "ОШИБКА") + "]");
            }
        }
        catch (Exception e) { Console.WriteLine("Ошибка: " + e.Message); }
    }

    static bool HasAny(string s, string pool)
    {
        foreach (char c in s) if (pool.IndexOf(c) >= 0) return true;
        return false;
    }

    // ---------------- Меню: парольная фраза ----------------
    static void PassphraseMenu()
    {
        Console.Write("Сколько слов: ");
        if (!int.TryParse(Console.ReadLine(), out int n))
        { Console.WriteLine("Не число."); return; }

        try
        {
            string ph = GeneratePassphrase(n);
            double bits = PassphraseBits(n);
            Console.WriteLine();
            Console.WriteLine("Фраза: " + ph);
            Console.WriteLine("Слов в словаре: " + words.Length);
            Console.Write("Стойкость: " + bits.ToString("F1") + " бит, оценка: ");
            Colored(Rating(bits));
            Console.WriteLine();

            // Сколько слов нужно, чтобы догнать 12-символьный пароль (71,5 бит).
            const double target = 71.5;
            int needOurs = (int)Math.Ceiling(target / Math.Log2(words.Length));
            int needDice = (int)Math.Ceiling(target / Math.Log2(7776));
            Console.WriteLine("До 71,5 бит: в нашем словаре нужно " + needOurs +
                              " слов, у Diceware (7776 слов) - " + needDice + ".");
        }
        catch (Exception e) { Console.WriteLine("Ошибка: " + e.Message); }
    }

    // ---------------- Меню: сравнение скоростей ----------------
    static void CompareSpeeds()
    {
        Console.WriteLine("Скорости: быстрый хеш 10 млрд/с, медленный (bcrypt-подобный) 10 тыс/с.");
        Console.WriteLine();
        string[] samples =
        {
            "12345678",
            "abcdefgh",
            "Abc123xy",
            "Abc123!@xyZ",
            "aB3!kL9@qW1#",
            "K7$mQ2!vX9#pL4@zR8"
        };
        foreach (var s in samples)
        {
            Console.WriteLine("Пароль: " + s +
                              " (длина " + s.Length + ", алфавит " + AlphabetSize(s) + ")");
            Console.WriteLine("  быстрый:  " + FormatTime(BruteSeconds(s, 1e10)));
            Console.WriteLine("  медленный: " + FormatTime(BruteSeconds(s, 1e4)));
            Console.WriteLine();
        }
        Console.WriteLine("Вывод для разработчика: без медленной хеш-функции");
        Console.WriteLine("даже короткие пароли подбираются быстро. Нужны bcrypt,");
        Console.WriteLine("scrypt или Argon2 со солью и достаточным числом итераций.");
    }

    // ---------------- Вспомогательные ----------------
    // Скрытый ввод со звёздочками.
    static string ReadMasked()
    {
        var sb = new StringBuilder();
        while (true)
        {
            ConsoleKeyInfo k;
            try { k = Console.ReadKey(true); }
            catch { return Console.ReadLine() ?? ""; }

            if (k.Key == ConsoleKey.Enter) break;
            if (k.Key == ConsoleKey.Backspace)
            {
                if (sb.Length > 0) { sb.Length--; Console.Write("\b \b"); }
            }
            else if (!char.IsControl(k.KeyChar))
            {
                sb.Append(k.KeyChar);
                Console.Write('*');
            }
        }
        return sb.ToString();
    }

    static bool Ask(string prompt, bool def)
    {
        Console.Write(prompt);
        string a = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (a.Length == 0) return def;
        return a.StartsWith("y") || a.StartsWith("д") || a == "1";
    }

    // Цветная оценка.
    static void Colored(string rating)
    {
        var old = Console.ForegroundColor;
        if (rating == "очень слабый" || rating == "слабый")
            Console.ForegroundColor = ConsoleColor.Red;
        else if (rating == "средний")
            Console.ForegroundColor = ConsoleColor.Yellow;
        else
            Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(rating);
        Console.ForegroundColor = old;
    }
}