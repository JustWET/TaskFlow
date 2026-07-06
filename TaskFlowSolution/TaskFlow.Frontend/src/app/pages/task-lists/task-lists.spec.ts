import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskLists } from './task-lists';

describe('TaskLists', () => {
  let component: TaskLists;
  let fixture: ComponentFixture<TaskLists>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskLists],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskLists);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
