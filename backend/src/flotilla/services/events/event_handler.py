import datetime
import time
from http.client import HTTPException
from logging import getLogger
from multiprocessing import Event
from typing import List
from xmlrpc.client import DateTime

import pytz
from pytest import Session
from requests import Response

from flotilla.database.crud import (
    create_report,
    read_event_by_status,
    read_robot_by_id,
    update_event_status,
)
from flotilla.database.db import SessionLocal
from flotilla.database.models import (
    EventDBModel,
    EventStatus,
    ReportStatus,
    RobotDBModel,
    RobotStatus,
)
from flotilla.services.isar.service import IsarService


def run_event_handler() -> None:
    event_handler = EventHandler()
    event_handler.main()


class EventHandler:
    def __init__(self):
        self.logger = getLogger("event handler")

        self.start_event_handler()

    def start_event_handler(self) -> None:
        self.logger.info(f"Event handler started")
        self.isar_service: IsarService = IsarService()
        return

    def main(self) -> None:
        while True:
            self.db_session: Session = SessionLocal()

            self.update_events_by_status()

            for event in self.db_events_pending:
                if self.event_ready_to_start(start_time=event.start_time):
                    try:
                        db_robot = self.get_robot_for_event(event=event)

                        response_isar = self.start_isar_mission(
                            robot=db_robot, event=event
                        )

                        self.logger.info(f"Starting mission:{event.echo_mission_id}")
                        self.set_event_status(
                            event_id=event.id, new_status=EventStatus.started
                        )

                        self.initialize_mission_report(
                            event=event, response_isar_json=response_isar.json()
                        )
<<<<<<< HEAD
                    except (HTTPException, Exception):
=======
                    except Exception:
>>>>>>> wip Implement EventHandler class
                        self.set_event_status(
                            event_id=event.id, new_status=EventStatus.failed
                        )
                        continue
            SessionLocal.remove()

    def update_events_by_status(self) -> None:
        self.db_events_pending: List[EventDBModel] = read_event_by_status(
            db=self.db_session, event_status=EventStatus.pending
        )

        self.db_events_completed: List[EventDBModel] = read_event_by_status(
            db=self.db_session, event_status=EventStatus.completed
        )

    def event_ready_to_start(self, start_time: DateTime) -> bool:
        current_time_utc = datetime.datetime.now(tz=datetime.timezone.utc)

        # Workaround to force timezone to be UTC if unspecified
        if not start_time.tzinfo:
            utc = pytz.UTC
            start_time = utc.localize(start_time)

        if current_time_utc < start_time:
            return False

        return True

    def get_robot_for_event(self, event: EventDBModel) -> RobotDBModel:
        try:
            db_robot: RobotDBModel = read_robot_by_id(
                db=self.db_session, robot_id=event.robot_id
            )

        except HTTPException as e:
            self.logger.error(
                f"Failed get robot for mission {event.echo_mission_id}, assigned robot id {event.robot_id}: {e.detail}"
            )

        if db_robot.status != RobotStatus.available:
            error_message = f"Robot {db_robot.id} is not available for scheduled event"
            # Should we check for other robot avilable?
            self.logger.error(error_message)
            raise Exception(error_message)

        return db_robot

    def start_isar_mission(self, robot: RobotDBModel, event: EventDBModel) -> None:
        try:
            response_isar: Response = self.isar_service.start_mission(
                host=robot.host,
                port=robot.port,
                mission_id=event.echo_mission_id,
            )

        except HTTPException as e:
            self.logger.error(
                f"Could not start mission with id {event.echo_mission_id} for robot with id {event.robot_id}: {e.detail}"
            )

        return response_isar

    def set_event_status(self, event_id, new_status: EventStatus) -> None:
        try:
            update_event_status(
                db=self.db_session,
                event_id=event_id,
                new_status=new_status,
            )
        except HTTPException as e:
            self.logger.error(f"Failed to set event status for event {event_id}")

    def initialize_mission_report(self, event: EventDBModel, response_isar_json) -> int:
        report_id: int = create_report(
            self.db_session,
            robot_id=event.robot_id,
            isar_mission_id=response_isar_json["mission_id"],
            echo_mission_id=event.echo_mission_id,
            report_status=ReportStatus.in_progress,
        )

        return report_id
