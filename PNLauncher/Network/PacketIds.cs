namespace PNLauncher.Network
{
    using System;

    public enum PacketIds : ushort
    {
        client_authorize = 0x6978,
        client_key_auth = 0x6d63,
        server_auth_result = 0x6979,
        server_disconnect = 0x7531,
        client_open_profile = 0x697a,
        server_profile_info = 0x697b,
        server_message = 0x7530,
        server_show_message = 0x7918,
        client_update_info = 0x59d8,
        server_update_info = 0x59d9,
        client_ls_info = 0x59da,
        server_ls_info = 0x59db,
        server_crush_report = 0x1388,
        client_runtime_param = 0x4a38,
        client_servers_info = 0x4e21,
        client_servers_ways_info = 0x4e2c,
        client_rtt_packet = 0xea60,
        server_rtt_packet = 0xea61,
        server_servers_info = 0x4e24,
        client_logout = 0x7148,
        client_change_pass = 0x697e,
        server_result_change_pass = 0x697f,
        client_wrong_files = 0x4e25,
        server_part_file = 0x4e26,
        server_parts_info = 0x4e27,
        server_start_file = 0x61a9,
        server_end_file = 0x61a8,
        server_update_error = 0x61b0,
        server_update_success = 0x61b2,
        server_servers_ways_info = 0x61b4,
        client_ready_to_start = 0x61aa,
        client_part_success = 0x61b1,
        server_urls_info = 0x4fdc,
        server_login_ip = 0x4fdd
    }
}

