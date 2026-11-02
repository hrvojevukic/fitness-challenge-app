import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { API_BASE_URL } from '../../core/api-config';
import { LeaderboardComponent } from './leaderboard.component';

describe('LeaderboardComponent', () => {
  let fixture: ComponentFixture<LeaderboardComponent>;
  let http: HttpTestingController;

  function respond(page: number): void {
    const request = http.expectOne((r) => r.url === `${API_BASE_URL}/leaderboard` && r.params.get('page') === String(page));
    request.flush({
      items: [{ userId: page, fullName: `User ${page}`, totalPoints: 10, rank: page, previousRank: null, trend: 'new' }],
      page,
      pageSize: 10,
      totalCount: 11,
      totalPages: 2,
    });
    fixture.detectChanges();
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [LeaderboardComponent],
      providers: [provideRouter([]), provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpTestingController);
    fixture = TestBed.createComponent(LeaderboardComponent);
    fixture.detectChanges();
  });

  afterEach(() => http.verify());

  it('loads the first page and then the page chosen in the pager', () => {
    respond(1);
    const element = fixture.nativeElement as HTMLElement;
    expect(element.textContent).toContain('User 1');

    const next = element.querySelectorAll<HTMLButtonElement>('app-pager button')[1];
    next.click();
    respond(2);

    expect(element.textContent).toContain('User 2');
    expect(element.textContent).toContain('Page 2 of 2');
  });
});
