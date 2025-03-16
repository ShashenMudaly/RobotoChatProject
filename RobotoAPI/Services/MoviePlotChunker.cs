using System;
using System.Collections.Generic;

namespace ChatApp.Services
{
    public static class MoviePlotChunker
    {
        private const int CHUNK_SIZE = 3600;
        private const int OVERLAP_SIZE = 140;

        public static List<string> ChunkPlot(string plot)
        {
            if (string.IsNullOrEmpty(plot))
                return new List<string>();

            var chunks = new List<string>();
            var position = 0;

            while (position < plot.Length)
            {
                var remainingLength = plot.Length - position;
                var currentChunkSize = Math.Min(CHUNK_SIZE, remainingLength);
                
                var chunk = plot.Substring(position, currentChunkSize);
                chunks.Add(chunk);

                // Move position for next chunk, accounting for overlap
                position += currentChunkSize - (position + currentChunkSize < plot.Length ? OVERLAP_SIZE : 0);
            }

            return chunks;
        }
    }
} 