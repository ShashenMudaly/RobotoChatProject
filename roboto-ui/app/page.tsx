"use client"

import { useState, useEffect, useRef } from "react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Card, CardContent, CardFooter, CardHeader, CardTitle } from "@/components/ui/card"
import { SendIcon } from "lucide-react"
import { API_BASE_URL } from "@/app/config/settings"
import Image from 'next/image'

// Preselected prompts that users can click on
const preselectedPrompts = [
  "In which movie does a group of thieves enter dreams to plant an idea?",
  "What happens in The Shawshank Redemption?",
  "Tell me about a movie where toys come to life when humans aren't around",
  "What's the plot of Fight Club?",
  "Describe a film where a boy discovers he's a wizard on his 11th birthday",
  "Why is The Silence of the Lambs considered a classic?",
]

export default function Chat() {
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const [messages, setMessages] = useState<Array<{ id: string; role: 'user' | 'assistant'; content: string }>>([])
  const [input, setInput] = useState('')
  const [isLoading, setIsLoading] = useState(false)

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setInput(e.target.value)
  }

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!input.trim()) return

    const userMessage = { id: Date.now().toString(), role: 'user' as const, content: input }
    setMessages(prev => [...prev, userMessage])
    setIsLoading(true)

    try {
      const response = await fetch('/api/chat', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          userId: '123456',
          query: input
        })
      })

      const data = await response.json()
      
      setMessages(prev => [...prev, {
        id: (Date.now() + 1).toString(),
        role: 'assistant',
        content: data.response
      }])
    } catch (error) {
      console.error('Error:', error)
    } finally {
      setIsLoading(false)
      setInput('')
    }
  }

  // Function to handle clicking on a preselected prompt
  const handlePromptClick = (prompt: string) => {
    setInput(prompt)
  }

  // Update submitPrompt to use the new handleSubmit
  const submitPrompt = (prompt: string) => {
    setInput(prompt)
    handleSubmit(new Event('submit', { cancelable: true }) as any)
  }

  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" })
  }

  useEffect(() => {
    scrollToBottom()
  }, [messages])

  return (
    <div className="flex items-center justify-center min-h-screen bg-gray-50 p-4">
      <Card className="w-[800px]">
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Image
              src="/bot-avatar.png"
              alt="Chat Bot"
              width={48}
              height={48}
              className="rounded-full"
            />
            Mr Roboto
          </CardTitle>
        </CardHeader>
        <CardContent className="h-[60vh] overflow-y-auto p-4 space-y-4">
          {messages.length === 0 && (
            <div className="flex h-full flex-col items-center justify-center text-center gap-4 p-4">
              <p className="text-muted-foreground">Send a message to start the conversation</p>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2 w-full max-w-md">
                {preselectedPrompts.map((prompt, index) => (
                  <Button
                    key={index}
                    variant="outline"
                    className="text-sm justify-start h-auto py-2 px-3 whitespace-normal text-left"
                    onClick={() => submitPrompt(prompt)}
                    disabled={isLoading}
                  >
                    {prompt}
                  </Button>
                ))}
              </div>
            </div>
          )}

          {messages.map((m) => (
            <div key={m.id} className={`flex ${m.role === "user" ? "justify-end" : "justify-start"}`}>
              <div
                className={`max-w-[80%] rounded-lg px-4 py-2 ${
                  m.role === "user" ? "bg-primary text-primary-foreground" : "bg-muted text-foreground"
                }`}
              >
                {m.content}
              </div>
            </div>
          ))}

          {isLoading && (
            <div className="flex justify-start">
              <div className="max-w-[80%] rounded-lg px-4 py-2 bg-muted">
                <div className="flex space-x-2">
                  <div className="h-2 w-2 rounded-full bg-muted-foreground/40 animate-bounce"></div>
                  <div className="h-2 w-2 rounded-full bg-muted-foreground/40 animate-bounce delay-75"></div>
                  <div className="h-2 w-2 rounded-full bg-muted-foreground/40 animate-bounce delay-150"></div>
                </div>
              </div>
            </div>
          )}
          
          <div ref={messagesEndRef} />
        </CardContent>

        {/* Preselected prompts section */}
        {messages.length > 0 && (
          <div className="px-4 py-2 border-t">
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
              {preselectedPrompts.map((prompt, index) => (
                <Button
                  key={index}
                  variant="outline"
                  size="sm"
                  className="text-xs h-auto py-1 px-2 justify-start whitespace-normal text-left"
                  onClick={() => handlePromptClick(prompt)}
                  disabled={isLoading}
                >
                  {prompt}
                </Button>
              ))}
            </div>
          </div>
        )}

        <CardFooter className="border-t p-4">
          <form onSubmit={handleSubmit} className="flex w-full space-x-2">
            <Input
              value={input}
              onChange={handleInputChange}
              placeholder="Type your message..."
              className="flex-grow"
              disabled={isLoading}
            />
            <Button type="submit" disabled={isLoading || !input.trim()}>
              <SendIcon className="h-4 w-4" />
              <span className="sr-only">Send</span>
            </Button>
          </form>
        </CardFooter>
      </Card>
    </div>
  )
}

