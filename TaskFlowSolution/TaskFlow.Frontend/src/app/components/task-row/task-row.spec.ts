import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskRow } from './task-row';

describe('TaskRow', () => {
  let component: TaskRow;
  let fixture: ComponentFixture<TaskRow>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskRow],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskRow);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
