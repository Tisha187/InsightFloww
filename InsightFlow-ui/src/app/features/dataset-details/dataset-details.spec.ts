import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DatasetDetails } from './dataset-details';

describe('DatasetDetails', () => {
  let component: DatasetDetails;
  let fixture: ComponentFixture<DatasetDetails>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [DatasetDetails],
    }).compileComponents();

    fixture = TestBed.createComponent(DatasetDetails);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
