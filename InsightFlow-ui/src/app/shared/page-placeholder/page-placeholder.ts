
import { Component, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-page-placeholder',
  standalone: true,
  templateUrl: './page-placeholder.html',
  styleUrl: './page-placeholder.css',
})
export class PagePlaceholder {
  protected readonly title = signal('Dashboard');
  protected readonly description = signal(
    'Your workspace for data analysis and insights.'
  );
  protected readonly eyebrow = signal('WORKSPACE');

  constructor(private readonly route: ActivatedRoute) {
    this.route.data.subscribe((data) => {
      this.title.set(data['title'] ?? 'Dashboard');
      this.description.set(
        data['description'] ??
          'Your workspace for data analysis and insights.'
      );
      this.eyebrow.set(data['eyebrow'] ?? 'WORKSPACE');
    });
  }
}
