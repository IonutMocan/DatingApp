import { Component, OnInit } from '@angular/core';
import { AdminService } from 'src/app/_service/admin.service';
import { AlertifyService } from 'src/app/_service/alertify.service';

@Component({
  selector: 'app-photo-management',
  templateUrl: './photo-management.component.html',
  styleUrls: ['./photo-management.component.css']
})
export class PhotoManagementComponent implements OnInit {
  photos: any;

  constructor(private adminService: AdminService, private aleritfy: AlertifyService) {}

  ngOnInit() {
    this.getPhotosForApproval();
  }

  getPhotosForApproval() {
    this.adminService.getPhotosForApproval().subscribe(
      (photos) => {
        this.photos = photos;
      },
      (error) => {
        this.aleritfy.error(error);
      }
    );
  }

  aprovePhoto(photoId) {
    this.adminService.approvePhoto(photoId).subscribe(
      () => {
        this.photos.splice(
          this.photos.findIndex((p) => p.id === photoId),
          1
        );
      },
      (error) => {
        this.aleritfy.error(error);
      }
    );
  }

  rejectPhoto(photoId) {
    this.adminService.rejectPhoto(photoId).subscribe(
      () => {
        this.photos.splice(
          this.photos.findIndex((p) => p.id === photoId),
          1
        );
      },
      (error) => {
        this.aleritfy.error(error);
      }
    );
  }
}
