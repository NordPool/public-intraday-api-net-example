/*
 *  Copyright 2017 Nord Pool.
 *  This library is intended to aid integration with Nord Pool�s Intraday API and comes without any warranty. Users of this library are responsible for separately testing and ensuring that it works according to their own standards.
 *  Please send feedback to idapi@nordpoolgroup.com.
 */

namespace Nordpool.ID.PublicApi.v1.Order.Error
{
	public enum ErrorCode 
	{
		MISSING_REQUIRED_FIELD,
		ILLEGAL_FIELD,
		FIELD_OUT_OF_RANGE,
		FIELD_FORMAT_INVALID,
		ITEM_NOT_FOUND,
		ACCESS_MODE_VIOLATION,
		THIRD_PARTY,
		INVALID_AREA,
		PARSING_ERROR,
		REUQEST_OVER_THROTTLING,
		INTERNAL_SERVER_ERROR,
	}
}
