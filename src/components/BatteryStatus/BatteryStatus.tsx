
import { Typography } from "@equinor/eds-core-react";
import { Icon } from "@equinor/eds-core-react/dist/types/components/Icon/Icon";

import { battery, battery_charging, battery_unknown, battery_alert } from '@equinor/eds-icons';

import { Battery, BatteryStatus } from "../../models/battery";
import styles from "./BatteryStatus.module.css";




export interface BatteryProps{
    status: Battery;
}


const BatteryStatusView = ({status}: BatteryProps): JSX.Element => {
    let battery_icon
    if (status === BatteryStatus.Normal) {
        battery_icon = battery
        }
    else if (status === BatteryStatus.Charging){ 
        battery_icon = battery_charging
        }
    else if (status === BatteryStatus.Critical){ 
        battery_icon = battery_alert
        }
    else if (status === BatteryStatus.Error){ 
        battery_icon = battery_unknown
        }
    return (
    <>
        <Icon name="{{}}" size={24} color="primary" />
    </>
    )
}




export default BatteryStatusView
