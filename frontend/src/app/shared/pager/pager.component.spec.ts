import { ComponentFixture, TestBed } from '@angular/core/testing';
import { PagerComponent } from './pager.component';

describe('PagerComponent', () => {
  let fixture: ComponentFixture<PagerComponent>;

  function render(page: number, totalPages: number): HTMLButtonElement[] {
    fixture.componentRef.setInput('page', page);
    fixture.componentRef.setInput('totalPages', totalPages);
    fixture.detectChanges();
    return Array.from((fixture.nativeElement as HTMLElement).querySelectorAll('button'));
  }

  beforeEach(() => {
    fixture = TestBed.createComponent(PagerComponent);
  });

  it('is hidden when there is only one page', () => {
    expect(render(1, 1).length).toBe(0);
  });

  it('disables Previous on the first page and Next on the last', () => {
    const [previous, next] = render(1, 3);
    expect(previous.disabled).toBeTrue();
    expect(next.disabled).toBeFalse();

    const [previousOnLast, nextOnLast] = render(3, 3);
    expect(previousOnLast.disabled).toBeFalse();
    expect(nextOnLast.disabled).toBeTrue();
  });

  it('emits the neighbouring page', () => {
    const emitted: number[] = [];
    fixture.componentInstance.pageChange.subscribe((page) => emitted.push(page));

    const [previous, next] = render(2, 3);
    previous.click();
    next.click();

    expect(emitted).toEqual([1, 3]);
  });
});
