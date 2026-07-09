import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TaskEditModal } from './task-edit-modal';

describe('TaskEditModal', () => {
  let component: TaskEditModal;
  let fixture: ComponentFixture<TaskEditModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TaskEditModal],
    }).compileComponents();

    fixture = TestBed.createComponent(TaskEditModal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
