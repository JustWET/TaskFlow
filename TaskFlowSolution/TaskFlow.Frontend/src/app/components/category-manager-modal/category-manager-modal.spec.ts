import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CategoryManagerModal } from './category-manager-modal';

describe('CategoryManagerModal', () => {
  let component: CategoryManagerModal;
  let fixture: ComponentFixture<CategoryManagerModal>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CategoryManagerModal],
    }).compileComponents();

    fixture = TestBed.createComponent(CategoryManagerModal);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
