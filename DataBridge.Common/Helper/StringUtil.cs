namespace DataBridge.Helper
{
    public static class StringUtil
    {
        /// <summary>
        /// Counts the character.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="toCount">To count.</param>
        /// <returns></returns>
        public static int CountCharacter(string str, char toCount)
        {
            int count = 0;
            for (int i = 0; i < str.Length; i++)
            {
                char chr = str[i];
                if (chr == toCount)
                {
                    count++;
                }
            }

            return count;
        }
    }
}