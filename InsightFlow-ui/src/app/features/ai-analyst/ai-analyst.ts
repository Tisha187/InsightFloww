import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface ChatMessage {
  id: number;
  role: 'user' | 'assistant';
  content: string;
  timestamp: Date;
}

interface Conversation {
  id: number;
  title: string;
  updatedAt: string;
}

@Component({
  selector: 'app-ai-analyst',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './ai-analyst.html',
  styleUrl: './ai-analyst.css',
})
export class AiAnalyst {
  selectedDataset = 'Sales Performance 2026';

  datasets = [
    'Sales Performance 2026',
    'Retail Transactions',
    'Customer Analytics',
  ];

  conversations: Conversation[] = [
    { id: 1, title: 'Sales performance overview', updatedAt: 'Today' },
    { id: 2, title: 'Regional revenue comparison', updatedAt: 'Yesterday' },
    { id: 3, title: 'Product performance analysis', updatedAt: 'Oct 30' },
  ];

  activeConversationId = 1;

  messages: ChatMessage[] = [
    {
      id: 1,
      role: 'assistant',
      content:
        'Hello! I am your InsightFlow AI Analyst. I can help you explore your datasets, understand business performance, identify trends, and answer questions using your data.\n\nWhat would you like to explore today?',
      timestamp: new Date(),
    },
  ];

  messageInput = '';
  isTyping = false;

  suggestedPrompts = [
    'Give me an overview of my sales performance.',
    'Which products generate the most revenue?',
    'Compare revenue across different regions.',
    'Identify important trends in my data.',
  ];

  sendMessage(): void {
    const content = this.messageInput.trim();

    if (!content || this.isTyping) {
      return;
    }

    this.messages.push({
      id: Date.now(),
      role: 'user',
      content,
      timestamp: new Date(),
    });

    this.messageInput = '';
    this.isTyping = true;

    // Temporary UI-only response.
    // This will be replaced with the AI Analyst API integration.
    setTimeout(() => {
      this.messages.push({
        id: Date.now() + 1,
        role: 'assistant',
        content:
          'I have received your question. The AI analysis service will be connected in the next development step.',
        timestamp: new Date(),
      });

      this.isTyping = false;
    }, 900);
  }

  usePrompt(prompt: string): void {
    this.messageInput = prompt;
    this.sendMessage();
  }

  startNewConversation(): void {
    this.activeConversationId = 0;
    this.messages = [
      {
        id: Date.now(),
        role: 'assistant',
        content:
          'Hello! What would you like to explore in your dataset today?',
        timestamp: new Date(),
      },
    ];
  }

  selectConversation(conversation: Conversation): void {
    this.activeConversationId = conversation.id;

    // Temporary placeholder until conversation history is connected.
    this.messages = [
      {
        id: Date.now(),
        role: 'assistant',
        content: `You selected "${conversation.title}". Conversation history will be loaded from the backend in a later step.`,
        timestamp: new Date(),
      },
    ];
  }
}
