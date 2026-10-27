import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Datasets } from './datasets';

describe('Datasets', () => {
  let component: Datasets;
  let fixture: ComponentFixture<Datasets>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Datasets],
    }).compileComponents();

    fixture = TestBed.createComponent(Datasets);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
