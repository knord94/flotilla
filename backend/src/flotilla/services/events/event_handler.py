import datetime
import time
from http.client import HTTPException
from logging import getLogger
from typing import List

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

logger = getLogger("event handler")


def event_ready_to_start(event: EventDBModel) -> bool:
    start_time = event.start_time
    current_time_utc = datetime.datetime.now(tz=datetime.timezone.utc)

    # Workaround to force timezone to be UTC if unspecified
    if not start_time.tzinfo:
        utc = pytz.UTC
        start_time = utc.localize(start_time)

    if current_time_utc < start_time:
        return False

    db_session: Session = SessionLocal()
    db_robot: List[RobotDBModel] = read_robot_by_id(
        db=db_session, robot_id=event.robot_id
    )
    SessionLocal.remove()
    if db_robot.status != RobotStatus.available:
        return False

    return True


def start_event_handler() -> None:
    logger.info(f"Event handler started")
    isar_service: IsarService = IsarService()
    while True:
        db_session: Session = SessionLocal()

        db_events_pending: List[EventDBModel] = read_event_by_status(
            db=db_session, event_status=EventStatus.pending
        )
        SessionLocal.remove()

        for event in db_events_pending:
            if event_ready_to_start(event=event):
                logger.info(event.echo_mission_id)
                try:
                    db_session: Session = SessionLocal()
                    robot: RobotDBModel = read_robot_by_id(
                        db=db_session, robot_id=event.robot_id
                    )
                    SessionLocal.remove()
                    response_isar: Response = isar_service.start_mission(
                        host=robot.host,
                        port=robot.port,
                        mission_id=event.echo_mission_id,
                    )

                    db_session: Session = SessionLocal()
                    update_event_status(
                        db=db_session, event_id=event.id, new_status=EventStatus.started
                    )
                    SessionLocal.remove()

                    db_session: Session = SessionLocal()
                    response_isar_json: dict = response_isar.json()
                    report_id: int = create_report(
                        db_session,
                        robot_id=event.robot_id,
                        isar_mission_id=response_isar_json["mission_id"],
                        echo_mission_id=event.echo_mission_id,
                        report_status=ReportStatus.in_progress,
                    )
                    SessionLocal.remove()

                except HTTPException as e:
                    logger.error(
                        f"Could not start mission with id {event.echo_mission_id} for robot with id {event.robot_id}: {e.detail}"
                    )
                    raise
        time.sleep(1)
