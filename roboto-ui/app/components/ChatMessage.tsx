import React from 'react';
import { cn } from '@/lib/utils';
import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';
import TypewriterText from './TypewriterText';

interface ChatMessageProps {
  message: string;
  isUser: boolean;
  avatar?: string;
}

const ChatMessage: React.FC<ChatMessageProps> = ({ message, isUser, avatar }) => {
  return (
    <div
      className={cn(
        'flex w-full gap-2 p-4',
        isUser ? 'flex-row-reverse' : 'flex-row'
      )}
    >
      <Avatar className="h-8 w-8">
        <AvatarImage src={avatar} />
        <AvatarFallback>
          {isUser ? 'U' : 'R'}
        </AvatarFallback>
      </Avatar>
      <div
        className={cn(
          'flex max-w-[80%] rounded-lg px-4 py-2',
          isUser
            ? 'bg-primary text-primary-foreground'
            : 'bg-muted'
        )}
      >
        {isUser ? (
          <span className="whitespace-pre-wrap">{message}</span>
        ) : (
          <TypewriterText text={message} speed={40} />
        )}
      </div>
    </div>
  );
};

export default ChatMessage; 