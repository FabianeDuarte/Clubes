namespace Clubes.Shared;

public static class Cpf
{
    public static string Limpar(string? s) => new string((s ?? "").Where(char.IsDigit).ToArray());

    public static bool Valido(string? valor)
    {
        var c = Limpar(valor);
        if (c.Length != 11 || c.Distinct().Count() == 1) return false;
        return Digito(c, 9) == c[9] - '0' && Digito(c, 10) == c[10] - '0';
    }

    static int Digito(string c, int n)
    {
        int soma = 0;
        for (int i = 0; i < n; i++) soma += (c[i] - '0') * (n + 1 - i);
        int r = soma * 10 % 11;
        return r == 10 ? 0 : r;
    }
}
