import { Component, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpHeaders } from '@angular/common/http';

// 👇 add this interface before the class
interface Message {
  role: 'user' | 'assistant';
  content: string;
}

@Component({
  selector: 'app-chat-widget',
  imports: [CommonModule, FormsModule],
  templateUrl: './chat-widget.html',
  styleUrl: './chat-widget.css',
})
export class ChatWidgetComponent { // 👈 renamed from ChatWidget

  @ViewChild('chatBox') chatBox!: ElementRef;

  isOpen = false;
  messages: Message[] = [];
  userInput = '';
  loading = false;

  private apiKey = 'YOUR_API_KEY_HERE';
  private apiUrl = 'https://api.anthropic.com/v1/messages';

  constructor(private http: HttpClient) {}

  toggleChat() {
    this.isOpen = !this.isOpen;
  }

  sendMessage() {
    const text = this.userInput.trim();
    if (!text || this.loading) return;

    this.messages.push({ role: 'user', content: text });
    this.userInput = '';
    this.loading = true;

    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      'x-api-key': this.apiKey,
      'anthropic-version': '2023-06-01'
    });

    const body = {
      model: 'claude-sonnet-4-6',
      max_tokens: 1024,
      messages: this.messages.map(m => ({ role: m.role, content: m.content }))
    };

    this.http.post<any>(this.apiUrl, body, { headers }).subscribe({
      next: (res) => {
        this.messages.push({ role: 'assistant', content: res.content[0].text });
        this.loading = false;
        this.scrollToBottom();
      },
      error: (err) => {
        this.messages.push({ role: 'assistant', content: 'Error: ' + err.message });
        this.loading = false;
      }
    });
  }

  private scrollToBottom() {
    setTimeout(() => {
      const el = this.chatBox?.nativeElement;
      if (el) el.scrollTop = el.scrollHeight;
    }, 50);
  }
}