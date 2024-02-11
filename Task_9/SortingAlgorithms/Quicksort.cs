namespace Task_9.SortingAlgorithms
{
    //From the internet
    class Quicksort
    {
        public char[] QuicksortLogic(char[] array, int leftIndex, int rightIndex)
        {
            var i = leftIndex;
            var j = rightIndex;
            var pivot = array[leftIndex];

            while (i <= j)
            {
                while (array[i] < pivot)
                {
                    i++;
                }

                while (array[j] > pivot)
                {
                    j--;
                }
                if (i <= j)
                {
                    var temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                    i++;
                    j--;
                }
            }

            if (leftIndex < j)
                QuicksortLogic(array, leftIndex, j);
            if (i < rightIndex)
                QuicksortLogic(array, i, rightIndex);
            return array;
        }
    }
}
