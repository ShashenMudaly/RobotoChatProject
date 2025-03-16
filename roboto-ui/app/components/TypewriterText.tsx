'use client';

import React, { useState, useEffect } from 'react';

interface TypewriterTextProps {
  text: string;
  speed?: number;
  onComplete?: () => void;
}

const TypewriterText: React.FC<TypewriterTextProps> = ({
  text,
  speed = 100,
  onComplete,
}) => {
  const [displayedText, setDisplayedText] = useState('');
  const [currentIndex, setCurrentIndex] = useState(0);
  const words = text.split(' ');

  // Function to determine if we should add a pause after the current word
  const shouldPause = (index: number) => {
    // Add longer pauses after punctuation
    if (/[.!?]$/.test(words[index])) {
      return 400; // Longer pause after sentences
    }
    if (/[,;:]$/.test(words[index])) {
      return 200; // Medium pause after clauses
    }
    // Random pauses between groups of words (roughly every 4-8 words)

    return 0;
  };

  useEffect(() => {
    // Reset when text changes
    setDisplayedText('');
    setCurrentIndex(0);
  }, [text]);

  useEffect(() => {
    if (currentIndex < words.length) {
      const pauseDuration = shouldPause(currentIndex);
      const timer = setTimeout(() => {
        // Add the next word plus a space (except for the last word)
        setDisplayedText(prev => 
          prev + words[currentIndex] + (currentIndex < words.length - 1 ? ' ' : '')
        );
        setCurrentIndex(prev => prev + 1);
      }, speed + pauseDuration);

      return () => clearTimeout(timer);
    } else if (onComplete) {
      onComplete();
    }
  }, [currentIndex, words, speed, onComplete]);

  return (
    <div className="whitespace-pre-wrap">
      {displayedText}
      {currentIndex < words.length && (
        <span className="animate-pulse">▋</span>
      )}
    </div>
  );
};

export default TypewriterText; 