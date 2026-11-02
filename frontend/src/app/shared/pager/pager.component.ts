import { Component, input, output } from '@angular/core';

@Component({
  selector: 'app-pager',
  standalone: true,
  templateUrl: './pager.component.html',
  styleUrl: './pager.component.css',
})
export class PagerComponent {
  page = input.required<number>();
  totalPages = input.required<number>();
  pageChange = output<number>();
}
