// Funzione XOR per cifrare/decifrare un byte array con chiave
// XOR semplice per polimorfismo base (offuscamento)
public static class XOREncryptor
{
    public static byte[] XorCipher(byte[] data, byte key)
    {
        byte[] output = new byte[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            output[i] = (byte)(data[i] ^ key);
        }
        return output;
    }
}
