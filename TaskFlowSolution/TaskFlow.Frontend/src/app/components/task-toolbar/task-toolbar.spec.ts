import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskToolbar } from './task-toolbar';

describe('TaskToolbar', () => {
  let component: TaskToolbar;
  let fixture: ComponentFixture<TaskToolbar>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskToolbar],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskToolbar);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
