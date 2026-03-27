import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-about',
  imports: [CommonModule, RouterModule],
  templateUrl: './about.html',
  styleUrl: './about.css',
})
export class AboutComponent {
    team = [
    { name: 'Khadija A.', role: 'Frontend Developer', initial: 'K' },
    { name: 'Shasha T.',   role: 'Backend Developer',  initial: 'S' },
    { name: 'Team Member', role: 'Full Stack',          initial: 'T' },
  ];
 
  values = [
    { icon: '🎯', title: 'Precision',    desc: 'We build systems that are accurate, reliable, and fast.' },
    { icon: '🤝', title: 'Collaboration', desc: 'Every feature is built as a team, for the community.' },
    { icon: '🌱', title: 'Innovation',   desc: 'We constantly improve our platform with new ideas.' },
  ];
}
